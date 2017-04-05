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

        // static List<string> ChangedRecipes = new List<string>();
        // static List<string> RemovedRecipes = new List<string>();


        static List<replaceWithStruct> replaceList;


        static void Main(string[] args)
        {
            string filepath = @"C:\Users\jonas\Documents\Minecraft\Instances\All the Mods (1)\minetweaker.log";

            // enabled path input
            if (true)
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

            readRecipesFromFile(filepath);
            initReplaceList();
            
            // only runs when there really is something to do
            if (replaceList.Count > 0)
            {
                // gets all recipes that gotta be changed
                List<string> RecipesToChange = searchRecipesToChange();
                Console.WriteLine("\nNormal Recipes: ");
                printRecipes(RecipesToChange);

                // gets the recipes changed and printed
                Console.WriteLine("\nChanged Recipes:");
                List<string> ChangedRecipes = changeRecipes(RecipesToChange);
                printRecipes(ChangedRecipes);

                // gets all the removed recipes
                List<string> RemovedRecipes = createRemoveRecipes(RecipesToChange, ref ChangedRecipes);

                //fixed both lists
                fixList(ref RemovedRecipes);
                fixList(ref ChangedRecipes);

                writeToFile("test.zs", RemovedRecipes, ChangedRecipes);
            }




            Console.ReadLine();
        }

        // reads all recipes from the file
        static void readRecipesFromFile(string source)
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
                        // only takes the actual recipes
                        if (line.StartsWith("[SERVER_STARTED][SERVER]recipes.add"))
                        {
                            AllRecipes.Add(line.Substring(24));
                        }

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

        // Outputs the given list into a file, lists separated by newline
        static void writeToFile(string path, params List<string>[] lists)
        {
            FileStream fs = null;
            try
            {
                if (File.Exists(path))
                {
                    if (File.Exists(path.Replace(".zs", "_old.zs"))){
                        File.Delete(path.Replace(".zs", "_old.zs"));
                    }
                    File.Move(path, path.Replace(".zs", "_old.zs"));
                }

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

        // inits all the recipes that are supposed to be replaced
        //TODO: Replace with read from config
        static void initReplaceList()
        {
            replaceList = new List<replaceWithStruct>();

            FileStream fs = null;

            try
            {
                // loads the config file
                fs = new FileStream(Environment.CurrentDirectory + "\\recipes.cfg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (StreamReader file = new StreamReader(fs))
                {
                    string line;
                    
                    while ((line = file.ReadLine()) != null)
                    {
                        // only takes the actual recipes
                        if (!line.StartsWith("#"))
                        {
                            string[] split = line.Split('>');
                            replaceList.Add(new replaceWithStruct("<" + split[0].Trim() + ">", "<" + split[1].Trim() + ">" ));
                        }

                    }
                    // checks if there is something in the file, otherwise adds something to it
                    if (replaceList.Count < 1)
                    {
                        Console.WriteLine(@"Replace List is empty, please add entries to the file. Stlye: [minecraft:iron_ingot>ore:ingotIron]. One entry per line");
                        if (fs.Length < 1)
                        {
                            using (StreamWriter writer = new StreamWriter(fs))
                            {
                                writer.WriteLine("#one entry per line");
                                writer.WriteLine("#minecraft:iron_ingot>ore:ingotIron");
                            }
                        }

                    }else
                    {
                        Console.WriteLine("\nTrying to replace {0} entries.\nThe following entries are getting replaced", replaceList.Count);
                        foreach (var l in replaceList)
                        {
                            Console.WriteLine(l.ToString());
                        }
                    }

                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }


            /*
            replaceList.Add(new replaceWithStruct("<railcraft:plate:1>", "<ore:plateSteel>"));
            replaceList.Add(new replaceWithStruct("<railcraft:plate>", "<ore:plateIron>"));
            replaceList.Add(new replaceWithStruct("<railcraft:plate:4>", "<ore:plateLead>"));
            replaceList.Add(new replaceWithStruct("<techreborn:techreborn.machineFrame>", "<ore:machineBlockBasic>"));
            replaceList.Add(new replaceWithStruct("<ic2:resource:12>", "<ore:machineBlockBasic>"));
            */
        }

        // goes through all the recipes and looks what has to be changed
        // returns list of elements that got to be changed
        static List<string> searchRecipesToChange()
        {
            List<string> toChangeList = new List<string>();

            for (int i = 0; i < AllRecipes.Count; i++)
            {
                foreach(replaceWithStruct r in replaceList)
                {
                    string s = AllRecipes[i];

                    if (s.Contains(r.origin) && !(s.StartsWith("recipes.addShaped(" + r.origin) || s.StartsWith("recipes.addShapeless(" + r.origin)))
                    {
                        toChangeList.Add(s);
                        AllRecipes[i] = "{This element got replaced}";
                        break;
                    }
                }
            }
            return toChangeList;
        }

        // prints out the given list 
        static void printRecipes(List<string> list)
        {
            foreach(string l in list)
            {
                Console.WriteLine(l);
            }
        }

        // changes all entries in that list and gives back a new, changed list
        static List<string> changeRecipes(List<string> recipesToChange)
        {
            List<string> changedRecipes = new List<string>();

            foreach(string r in recipesToChange)
            {
                string changeMe = r;

                foreach (replaceWithStruct s in replaceList)
                {
                    changeMe = changeMe.Replace(s.origin, s.replaceWith);
                }
                changedRecipes.Add(changeMe);
            }

            return changedRecipes;
        }

        // creates the list of code to remove it
        static List<string> createRemoveRecipes(List<string> recipesToChange, ref List<string> changedRecipes)
        {
            List<string> removedRecipes = new List<string>();

            foreach (var recipe in recipesToChange)
            {
                // checks whether the are other crafting recipes for the thing, if yes, then adds it back in
                string substring = recipe.Substring(0, recipe.IndexOf(">") + 1);

                Console.WriteLine("substring: " + substring);
                foreach(string line in AllRecipes)
                {
                    if (line.Contains(substring) && !recipesToChange.Contains(line) && !changedRecipes.Contains(line))
                    {
                        changedRecipes.Add(line);
                    }
                }
                removedRecipes.Add(recipe.Replace("add", "remove"));

            }

            return removedRecipes;
        }

        // fix list 
        static void fixList(ref List<string> brokenList)
        {
            for (int i = 0; i < brokenList.Count; i++)
            {
                brokenList[i] = brokenList[i].Replace(".withTag({})", "");

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
        public override string ToString()
        {
            return origin + " --> " + replaceWith;
        }

        public string origin;
        public string replaceWith;
    }

}
