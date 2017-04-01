using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeReplacerMT
{
    class Program
    {

        static List<string> AllRecipes = new List<string>();
        static List<string> RecipesToChange = new List<string>();
        static List<string> ChangedRecipes = new List<string>();
        static List<string> RemovedRecipes = new List<string>();


        static List<replaceWithStruct> replaceList;


        static void Main(string[] args)
        {
            string filepath = @"C:\Users\jonas\Documents\Minecraft\Instances\All the Mods (1)\minetweaker.log";


            if (false)
            {
                Console.WriteLine("Insert Filepath");
                filepath = Console.ReadLine();
                if (filepath.Last() == '"')
                {
                    filepath = filepath.Remove(filepath.Length - 1, 1);
                }

                if (filepath.First() == '"')
                {
                    filepath = filepath.Remove(0, 1);
                }
            }
            readText(filepath);
            initReplaceList();


            searchRecipesToChange();
            Console.WriteLine("\nNormal Recipes: ");
            printRecipes(RecipesToChange);

            Console.WriteLine("\nChanged Recipes:");
            changeRecipes();
            printRecipes(ChangedRecipes);
            createRemoveRecipes();
            writeToFile("test.zs", RemovedRecipes, ChangedRecipes);



            Console.ReadLine();
        }


        static void readText(string source)
        {
            FileStream fs = null;
            AllRecipes.Clear();

            try
            {
                fs = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader file = new StreamReader(fs))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        AllRecipes.Add(line);
                    }

                    Console.WriteLine("\nThe file had {0} lines.", AllRecipes.Count);
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }

        }

        static void writeToFile(string path, params List<string>[] lists)
        {
            FileStream fs = null;

            try
            {
                

                fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (StreamWriter file = new StreamWriter(fs))
                {

                    foreach (var list in lists)
                    {
                        foreach (string line in list)
                        {
                            file.WriteLine(line);
                        }
                        file.WriteLine();
                    }
                    
                    // Console.WriteLine("\nThe file has {0} lines.", list.Count);
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }
        }

        static void initReplaceList()
        {
            replaceList = new List<replaceWithStruct>();
            replaceList.Add(new replaceWithStruct("<railcraft:plate:1>", "<ore:plateSteel>"));
            replaceList.Add(new replaceWithStruct("<railcraft:plate>", "<ore:plateIron>"));
            replaceList.Add(new replaceWithStruct("<railcraft:plate:4>", "<ore:plateLead>"));


        }

        static void searchRecipesToChange()
        {
            foreach(string line in AllRecipes)
            {
                foreach(replaceWithStruct r in replaceList)
                {
                    string s = line.Replace("[SERVER_STARTED][SERVER]", "");
                    if (s.Contains(r.origin) && !(s.StartsWith("recipes.addShaped(" + r.origin) || s.StartsWith("recipes.addShapeless(" + r.origin)))
                    {

                        RecipesToChange.Add(s);
                        break;
                    }
                }


            }
        }

        static void printRecipes(List<string> list)
        {
            foreach(string l in list)
            {
                Console.WriteLine(l);
            }
        }

        static void changeRecipes()
        {
            foreach(string r in RecipesToChange)
            {
                string changeMe = r;
                foreach(replaceWithStruct s in replaceList)
                {
                    changeMe = changeMe.Replace(s.origin, s.replaceWith);
                }
                ChangedRecipes.Add(changeMe);
            }
        }

        static void createRemoveRecipes()
        {
            foreach (var recipe in RecipesToChange)
            {
                RemovedRecipes.Add(recipe.Replace("add", "remove"));
            }
        }
    }



    struct replaceWithStruct
    {
        public replaceWithStruct(string origin, string replaceWith)
        {
            this.origin = origin;
            this.replaceWith = replaceWith;
        }

        public string origin;
        public string replaceWith;
    }

}
