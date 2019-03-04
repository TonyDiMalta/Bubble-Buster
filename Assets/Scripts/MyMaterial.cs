using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// A helper class to cache our color name along with the current material
    /// </summary>
    class MyMaterial : Material
    {
        public string ColorName { get; set; }

        public MyMaterial(Material baseMaterial)
            : base(baseMaterial)
        { }

        public MyMaterial(Material baseMaterial, string colorName)
            : base(baseMaterial)
        {
            ColorName = colorName;
        }
        
        private static MyMaterial[] cachedMaterials =
        {
            new MyMaterial(Resources.Load("Materials/redMaterial") as Material, "red"),
            new MyMaterial(Resources.Load("Materials/greenMaterial") as Material, "green"),
            new MyMaterial(Resources.Load("Materials/blueMaterial") as Material, "blue"),
            new MyMaterial(Resources.Load("Materials/yellowMaterial") as Material, "yellow"),
            new MyMaterial(Resources.Load("Materials/purpleMaterial") as Material, "purple")
        };

        /// <summary>
        /// helper method to get a random color
        /// </summary>
        /// <returns></returns>
        public static MyMaterial GetRandomMaterial()
        {
            int index = Random.Range(0, cachedMaterials.Length);
            return cachedMaterials[index];
        }
    }
}
