using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TGStylizedWorld
{
    public class TGCharacterAppearance : MonoBehaviour
    {
        [System.Serializable]
        public class TGHairItem
        {
            public string Name;
            public SkinnedMeshRenderer Hair;
            public List<Material> HairSkins = new List<Material>();
            public bool AllowHair = true;
            public bool AllowHat=true;
            public bool AllowGlasses = true;
        }

        [System.Serializable]
        public enum TGPartMode { Random, Visible, Hidden}
        [System.Serializable]
        public enum TGPartSkinMode { Random, Default, DontChange }
        [System.Serializable]
        public enum TGHairSkin { Black, Brown, Blond, Gray}
        [System.Serializable]
        public enum TGBodySkins { Light, Medium, Dark }
        [System.Serializable]
        public enum TGClothSkins { Skin1, Skin2, Skin3, Skin4, Skin5, Skin6 }

        [Header("-CHARACTER PARTS-")]
        public SkinnedMeshRenderer Character;
        public SkinnedMeshRenderer Glasses;
        public SkinnedMeshRenderer Hat;

        [Header("-HAIRS-")]
        public List<TGHairItem> Hairs = new List<TGHairItem>();
        public TGHairSkin HairsDefaultSkin = TGHairSkin.Black;
        public List<TGHairSkin> HairsRandomSkins = new List<TGHairSkin>();

        [Header("-BODY-")]
        public List<Material> BodySkins = new List<Material>();
        public TGBodySkins BodyDefaultSkin = TGBodySkins.Light;
        public List<TGBodySkins> BodyRandomSkins = new List<TGBodySkins>();

        [Header("-CLOTHES-")]
        public List<Material> ClothSkins = new List<Material>();
        public TGClothSkins ClothDefaultSkin = TGClothSkins.Skin1;
        public List<TGClothSkins> ClothRandomSkins = new List<TGClothSkins>();

        [Header("-MODES-")]
        public TGPartMode HairsMode =TGPartMode.Random;
        public TGPartSkinMode HairsSkinMode = TGPartSkinMode.Random;
        public TGPartMode GlassesMode = TGPartMode.Random;
        public TGPartMode HatMode = TGPartMode.Random;
        public TGPartSkinMode ClothSkinMode = TGPartSkinMode.Random;
        public TGPartSkinMode BodySkinMode = TGPartSkinMode.Random;

        bool doRagdollOn=false;

        private void Reset()
        {
            // Change All Rigid bodies to Kinematic
            Rigidbody[] rigids = GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody r in rigids)
            {
                r.isKinematic = true;
#if UNITY_EDITOR
                EditorUtility.SetDirty(r.gameObject);
#endif
            }

           // Get all childs
            SkinnedMeshRenderer[] childs = GetComponentsInChildren<SkinnedMeshRenderer>();
            Hairs.Clear();
            BodySkins.Clear();
            ClothSkins.Clear();
            HairsRandomSkins.Add(TGHairSkin.Black);
            HairsRandomSkins.Add(TGHairSkin.Brown);
            HairsRandomSkins.Add(TGHairSkin.Blond);
            HairsRandomSkins.Add(TGHairSkin.Gray);
            foreach (SkinnedMeshRenderer child in childs)
            {
                // Add hairs to list
                if (child.name.Contains("Hairs"))
                {
                    TGHairItem item = new TGHairItem();
                    item.Name = child.name;
                    item.Hair = child;
#if UNITY_EDITOR
                    string path=AssetDatabase.GetAssetPath(item.Hair.sharedMaterial);
                    path=path.Remove(path.Length - 5, 5);
                    Material hairSkin=(Material)AssetDatabase.LoadMainAssetAtPath(path + "A.mat");
                    item.HairSkins.Add(hairSkin);
                    hairSkin = (Material)AssetDatabase.LoadMainAssetAtPath(path + "B.mat");
                    item.HairSkins.Add(hairSkin);
                    hairSkin = (Material)AssetDatabase.LoadMainAssetAtPath(path + "C.mat");
                    item.HairSkins.Add(hairSkin);
                    hairSkin = (Material)AssetDatabase.LoadMainAssetAtPath(path + "D.mat");
                    item.HairSkins.Add(hairSkin);
#endif
                    Hairs.Add(item);
                }                        
                if (child.name=="Glasses")
                {
                    Glasses = child;
                }

                if (child.name=="Hat")
                {
                    Hat = child;
                }

                if (child.name.Contains("0") && child.name.Contains("_"))
                {
                    Character = child;
                }
            }
            // Clothes and Body skins
            if (Character)
            {
#if UNITY_EDITOR
                string clothesPath = AssetDatabase.GetAssetPath(Character.sharedMaterials[0]);
                clothesPath = clothesPath.Remove(clothesPath.Length - 5, 5);
                Material clothSkin= (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "A.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin1);

                clothSkin = (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "B.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin2);

                clothSkin = (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "C.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin3);

                clothSkin = (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "D.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin4);

                clothSkin = (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "E.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin5);

                clothSkin = (Material)AssetDatabase.LoadMainAssetAtPath(clothesPath + "F.mat");
                ClothSkins.Add(clothSkin);
                if (clothSkin) ClothRandomSkins.Add(TGClothSkins.Skin6);


                string bodyPath= AssetDatabase.GetAssetPath(Character.sharedMaterials[1]);
                bodyPath = bodyPath.Remove(bodyPath.Length - 5, 5);
                Material bodySkin = (Material)AssetDatabase.LoadMainAssetAtPath(bodyPath + "A.mat");
                BodySkins.Add(bodySkin);
                if (bodySkin) BodyRandomSkins.Add(TGBodySkins.Light);

                bodySkin = (Material)AssetDatabase.LoadMainAssetAtPath(bodyPath + "B.mat");
                BodySkins.Add(bodySkin);
                if (bodySkin) BodyRandomSkins.Add(TGBodySkins.Medium);

                bodySkin = (Material)AssetDatabase.LoadMainAssetAtPath(bodyPath + "C.mat");
                BodySkins.Add(bodySkin);
                if (bodySkin) BodyRandomSkins.Add(TGBodySkins.Dark);
#endif
            }
        }



        // Start is called before the first frame update
        void Start()
        {            
            bool hatEnabled = true;
            bool glassesEnabled = true;
            TGHairItem hairItem=null;
            // Hairs mode
            switch (HairsMode)
            {
                case TGPartMode.Random:
                    // get all allowed hairs
                    List<int> hairIndexes = new List<int>();
                    int i = 0;
                    foreach (TGHairItem item in Hairs)
                    {
                        if (item.AllowHair) hairIndexes.Add(i);
                        i++;
                    }
                    // Show random hair
                    int hairIndex = Random.Range(0, hairIndexes.Count - 1);
                    foreach (TGHairItem item in Hairs)
                    {
                        item.Hair.gameObject.SetActive(false);
                    }
                    hairItem = Hairs[hairIndexes[hairIndex]];
                    hatEnabled = hairItem.AllowHair;
                    glassesEnabled = hairItem.AllowGlasses;
                    hairItem.Hair.gameObject.SetActive(true);
                    break;
                case TGPartMode.Hidden:
                    foreach (TGHairItem item in Hairs)
                    {
                        item.Hair.gameObject.SetActive(false);
                    }
                    break;
                case TGPartMode.Visible:
                    foreach (TGHairItem item in Hairs)
                    {
                        item.Hair.gameObject.SetActive(false);
                    }
                    Hairs[0].Hair.gameObject.SetActive(true);
                    hairItem = Hairs[0];
                    break;
            }
            // hair color
            if (hairItem!=null)
            {
                switch (HairsSkinMode)
                {
                    case TGPartSkinMode.Random:
                        int hairColorIndex = Random.Range(0, HairsRandomSkins.Count);
                        if (hairColorIndex < HairsRandomSkins.Count && HairsRandomSkins.Count > 0)
                        {
                            if (hairItem.HairSkins[(int)HairsRandomSkins[hairColorIndex]])
                            {
                                hairItem.Hair.material = hairItem.HairSkins[(int)HairsRandomSkins[hairColorIndex]];
                            }
                        }
                        break;
                    case TGPartSkinMode.Default:
                        hairItem.Hair.material = hairItem.HairSkins[(int)HairsDefaultSkin];
                        break;
                }
            }
            // Hat mode
            if (Hat)
            {                
                switch (HatMode)
                {
                    case TGPartMode.Random:
                        Hat.gameObject.SetActive(Random.Range(0, 2) > 0 ? hatEnabled : false);
                        break;
                    case TGPartMode.Visible:
                        Hat.gameObject.SetActive(true);
                        break;
                    case TGPartMode.Hidden:
                        Hat.gameObject.SetActive(false);
                        break;
                }
            }

            // Glasses mode
            if (Glasses)
            {
                switch (GlassesMode)
                {
                    case TGPartMode.Random:
                        Glasses.gameObject.SetActive(Random.Range(0, 2) > 0 ? glassesEnabled : false);
                        break;
                    case TGPartMode.Visible:
                        Glasses.gameObject.SetActive(glassesEnabled);
                        break;
                    case TGPartMode.Hidden:
                        Glasses.gameObject.SetActive(false);
                        break;
                }
            }

            int clothSkinID=0;
            // Cloth Skin Mode
            switch (ClothSkinMode)
            {
                case TGPartSkinMode.Random:
                    int clothIndex = Random.Range(0, ClothRandomSkins.Count);
                    if (clothIndex< ClothRandomSkins.Count && ClothRandomSkins.Count>0)
                        clothSkinID = (int) ClothRandomSkins[clothIndex];
                    break;
                case TGPartSkinMode.Default:
                    clothSkinID = (int)ClothDefaultSkin;
                    break;
                case TGPartSkinMode.DontChange:
                    clothSkinID = 100;
                    break;
            }
            // apply skin
            if (clothSkinID < ClothSkins.Count && ClothSkins.Count>0)
            {
                if (ClothSkins[clothSkinID])
                {
                    Character.material = ClothSkins[clothSkinID];
                    if (Glasses)
                        Glasses.material = ClothSkins[clothSkinID];
                    if (Hat)
                        Hat.material = ClothSkins[clothSkinID];
                }
            }


            // Body Skin Mode
            switch (BodySkinMode)
            {
                case TGPartSkinMode.Random:
                    int bodyIndex = Random.Range(0, BodyRandomSkins.Count);
                    if (bodyIndex < BodyRandomSkins.Count && BodyRandomSkins.Count > 0)
                    {
                        if (BodySkins[(int)BodyRandomSkins[bodyIndex]])
                        {
                            Material[] mats = Character.materials;
                            mats[1] = BodySkins[(int)BodyRandomSkins[bodyIndex]];
                            Character.materials = mats;
                        }
                    }
                    break;
                case TGPartSkinMode.Default:
                    if ((int)BodyDefaultSkin < BodySkins.Count && BodySkins.Count > 0)
                    {
                        if (BodySkins[(int)BodyDefaultSkin])
                        {
                            Material[] mats = Character.materials;
                            mats[1] = BodySkins[(int)BodyDefaultSkin];
                            Character.materials = mats;
                        }
                    }
                    break;
                case TGPartSkinMode.DontChange:
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}