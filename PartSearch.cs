using System;
using System.Collections.Generic;
using System.Linq;
using MSCLoader;
using HutongGames.PlayMaker;
using Newtonsoft.Json;
using UnityEngine;

namespace PartSearch {
    public class PartSearch : Mod {
        public override string ID => "PartSearch"; // Your (unique) mod ID 
        public override string Name => "PartSearch"; // Your mod name
        public override string Author => "Krutonium"; // Name of the Author (your name)
        public override string Version => "1.0"; // Version
        public override string Description => "Find that Part!"; // Short description of your mod
        public override Game SupportedGames => Game.MyWinterCar; //Supported Games
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, DoSearch);
            SetupFunction(Setup.ModSettings, ModSettings);
        }

        private Material highlightMat;
        HashSet<GameObject> parts;
        Dictionary<Renderer, Material[]> originalMaterials =
            new Dictionary<Renderer, Material[]>();

        private bool applied = false;
        private string searchText;
        private void DoSearch()
        {
            if (FindItems.GetKeybindDown())
            {
                if (!applied)
                {
                    CreatePopupWindow();
                }
                else
                {
                    ClearHighlight();
                }
                applied = !applied;
            }
        }

        class Part
        {
            public string partName;
        }
        private void DoActualWork(string SearchText)
        {
            //ModConsole.Print(SearchText);
            var partObj = JsonConvert.DeserializeObject<Part>(SearchText);
            parts = new HashSet<GameObject>();
            Transform player = GameObject.Find("PLAYER").transform;
            Collider[] hits = Physics.OverlapSphere(
                player.position,
                20,
                ~0
            );
            foreach (Collider hit in hits)
            {
                if (hit.name.Contains("VINXX") && hit.name.ToLower().Contains(partObj.partName.ToLower()))
                {
                    parts.Add(hit.gameObject);
                }
            }
            ApplyGreenHighlight(parts.ToList());
        }
        private void CreatePopupWindow()
        {
            GameObject settingsMenu = GameObject.Find("Systems").transform.Find("OptionsMenu").gameObject;
            settingsMenu.SetActive(true);
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = true;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerStop").Value = true;
            PopupSetting popupSetting = ModUI.CreatePopupSetting("Parts Search", "Search");
            popupSetting.AddTextBox("partName", "Part Name", string.Empty, "Name of the Part");
            settingsMenu.SetActive(false);
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerStop").Value = false;
            popupSetting.ShowPopup(DoActualWork);
        }

        
        void ApplyGreenHighlight(List<GameObject> objects)
        {
            foreach (var go in objects)
            {
                var renderers = go.GetComponentsInChildren<Renderer>(true);

                foreach (var rend in renderers)
                {
                    if (rend == null)
                        continue;

                    if (!originalMaterials.ContainsKey(rend))
                        originalMaterials[rend] = rend.materials;

                    Material[] mats = new Material[rend.materials.Length];
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = highlightMat;

                    rend.materials = mats;
                }
            }
        }
        void ClearHighlight()
        {
            foreach (var kv in originalMaterials)
            {
                if (kv.Key != null)
                    kv.Key.materials = kv.Value;
            }

            originalMaterials.Clear();
        }


        private SettingsKeybind FindItems;
        private void ModSettings()
        {
            FindItems = Keybind.Add("Search", "Search", KeyCode.Alpha4);
        }
        
        private void Mod_OnLoad()
        {
            Shader shader = Shader.Find("Standard");
            highlightMat = new Material(shader);

            // Bright MSC-green
            Color green = new Color(0.2f, 1f, 0.2f, 1f);

            highlightMat.color = green;
            highlightMat.EnableKeyword("_EMISSION");
            highlightMat.SetColor("_EmissionColor", green * 2.5f);

            highlightMat.SetFloat("_Glossiness", 0.0f);
            highlightMat.SetFloat("_Metallic", 0.0f);
            
        }
    }
}