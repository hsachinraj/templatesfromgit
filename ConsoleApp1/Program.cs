using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Newtonsoft;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace ConsoleApp1
{


    public class LabTemplates
    {
        public string name { get; set;  }
        public string key { get; set;  }
        public string TemplateFolder { get; set;  }

        public string Description { get; set;  }
        public string Message { get; set;  }

        public string image { get; set; }

        public string tags { get; set; }
    }

    public class LabTemplateFiles
    {
        public string name { get; set; }
        public string path { get; set; }
        public string size { get; set; }
        public string url { get; set; }

        public string DownloadUrl { get; set; }

    }

    public class LabTemplateInfo
    {
        public string name { get; set; }
        public string Publisher { get; set; }
        [JsonProperty(PropertyName = "TemplateFiles")]
        public TemplateFiles labTemplateFile { get; set; }
    }

    public class TemplateFiles
    {
        public string Teams { get; set; }

        [JsonProperty(PropertyName = "Source Code")]
        public string sourcecode { get; set; }
        [JsonProperty(PropertyName = "Board Columns")]
        public string BoardColumns { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {


            const string GitHub_Repository = "demotemplates";
            const string GitHub_Owner = "hsachinraj";
            try
            {
                var client = new GitHubClient(new Octokit.ProductHeaderValue("hsachinraj"));
                 //string json = Newtonsoft.Json.JsonConvert.SerializeObject(repo.ToArray());
                //System.IO.File.WriteAllText("mycontents.json", json);
                //display all files and folders in the repo
                /* for (int i =0; i< repo.Count; i++)
                 {
                     Console.WriteLine("File tyep is {0} and URL is {1}", repo.ElementAt(i).Type, repo.ElementAt(i).Url);
                } */

                Console.WriteLine("------------------------------------------------------------------------------------");
                Console.WriteLine("List of templates available. Select the template which you want to deploy");
                //Get contents from template sesstings JSON file
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/hsachinraj/demotemplates/master/TemplateSetting.json");
                request.ContentType = "application/json; charset=utf-8";


                using (var response = request.GetResponse().GetResponseStream())
                {
                    using (var streamread = new StreamReader(response))
                    {
                        var TemplatesasJSON = streamread.ReadToEnd();
                        var Templates = JsonConvert.DeserializeObject<List<LabTemplates>>(TemplatesasJSON);
                        int templateNumber = 0;
                        foreach (var template in Templates)
                        {
                            Console.WriteLine("{2}. Template Name {0} - Key {1}", template.name, template.key, templateNumber++);

                        }
                        Console.WriteLine("------------------------------------------------------------------------------------");
                        Console.WriteLine("");


                        int selectedTemplateNumber;
                        bool validTemplateSelected = false;
                        selectedTemplateNumber = Console.ReadKey().KeyChar;
                        do
                        {

                            if (selectedTemplateNumber >= 48 && selectedTemplateNumber <= 59)
                            {
                                selectedTemplateNumber = selectedTemplateNumber - 48;
                                if (selectedTemplateNumber <= templateNumber-1)
                                {
                                    validTemplateSelected = true;
                                    break;
                                }
                            }
                            Console.WriteLine("");
                            Console.WriteLine("Enter a valid template number from 1 to {0}", templateNumber-1);
                            selectedTemplateNumber = Console.ReadKey().KeyChar;
                        } while (validTemplateSelected == false);

                        
                        string selecedTemplatePath = Templates.ElementAt(selectedTemplateNumber - 1).TemplateFolder;
                        Console.WriteLine(" Selected Template is located in {0}", selecedTemplatePath);
                        getTemplateDetails(GitHub_Owner, GitHub_Repository, selecedTemplatePath, client);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();

        }

        private static void getTemplateDetails(string Owner, string Repository, string TemplatePath, GitHubClient client)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/" + Owner + "/" + Repository + "/master/" + TemplatePath + "/ProjectTemplate.json");
            request.ContentType = "application/json; charset=utf-8";
            var response = request.GetResponse().GetResponseStream();
            var streamread = new StreamReader(response);
            var TemplatesasJSON = streamread.ReadToEnd();
            var templateInfo = JsonConvert.DeserializeObject<LabTemplateInfo>(TemplatesasJSON);

            var templateFiles = client.Repository.Content.GetAllContents(Owner, Repository, TemplatePath).Result;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(templateFiles.ToArray());
            var templateItems = JsonConvert.DeserializeObject<List<LabTemplateFiles>>(json);

            Console.WriteLine("Template Name {0} - Publisher URL {1}", templateInfo.name, templateInfo.Publisher);
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("Template         Template Files                                          Found?");
            Console.WriteLine("Teams            {0}                                                     {1}", templateInfo.labTemplateFile.BoardColumns, templateFileExists(templateInfo.labTemplateFile.BoardColumns.ToString(), templateItems));
            Console.WriteLine("Teams            {0}                                                     {1}", templateInfo.labTemplateFile.sourcecode, templateFileExists(templateInfo.labTemplateFile.sourcecode.ToString(), templateItems));
            Console.WriteLine("Teams            {0}                                                     {1}", templateInfo.labTemplateFile.Teams, !(string.IsNullOrEmpty(templateItems.Find(x => x.name == templateInfo.labTemplateFile.Teams).DownloadUrl)));

        }

        private static bool templateFileExists(string itemToSearch, List<LabTemplateFiles> templateItems )
        {
            try
            {
                if (!(string.IsNullOrEmpty(templateItems.Find(x => x.name == itemToSearch).DownloadUrl)))
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }


    }

}
