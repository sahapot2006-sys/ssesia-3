using System;
using System.Collections.Generic;

namespace Technolog.Models
{
    public class Recipe
    {
        public int recipe_id { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public List<RecipeComposition> composition { get; set; }
    }

    public class RecipeComposition
    {
        public int composition_id { get; set; }
        public int recipe_id { get; set; }
        public int component_id { get; set; }
        public string component_name { get; set; }
        public decimal percentage { get; set; }
        public int load_order { get; set; }
    }

    public class RecipeCreateModel
    {
        public string name { get; set; }
        public int version { get; set; }
    }

    public class CompositionModel
    {
        public int component_id { get; set; }
        public decimal percentage { get; set; }
        public int load_order { get; set; }
    }
}