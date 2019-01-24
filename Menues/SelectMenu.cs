using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inn2PowerDataStreamUpdater.Misc;

namespace Inn2PowerDataStreamUpdater.Menues
{
    public class SelectMenu
    {
        private readonly List<APICompany> _companies;
        private List<APICompany> _selectedCompanies;

        public SelectMenu(List<APICompany> companies)
        {
            this._companies = companies;
            this._selectedCompanies = new List<APICompany>();
        }

        /// <summary>
        /// Runs this menu and funtkions.
        /// </summary>
        /// <returns></returns>
        public SelectMenuResultObject RunMenu()
        {
            var badresult = new SelectMenuResultObject();
            var succesresult = new SelectMenuResultObject();

            var done = false;
            var finish = false;
            var back = false;
            while (!done)
            {
                MenuDialog();
                var input = Console.ReadLine().ToLower().Trim();

                switch (input)
                {
                    case "p":
                        PrintSelected();
                        break;
                    case "f":
                        finish = true;
                        done = true;
                        break;
                    case "b":
                        back = true;
                        done = true;
                        break;
                    default:
                        SelectCompany(InputSplitter(input));
                        break;
                }                  
            }
            if (finish)
            {
                succesresult.IsSuccesFull = true;
                succesresult.SelectedCompanies = this._selectedCompanies;
                succesresult.Companies = this._companies;
                return succesresult;
            }
            else
            {
                succesresult.IsSuccesFull = true;
                succesresult.SelectedCompanies = new List<APICompany>();
                succesresult.Companies = this._companies;
                return succesresult;
            }                      
        }

        /// <summary>
        /// Splits the line input into Chars via ,
        /// and colelcts them in a list.
        /// </summary>
        /// <param name="input">The console input</param>
        /// <returns>List of inputs</returns>
        private List<int> InputSplitter(string input)
        {
            var cleaninput = input.ToLower().Trim();
            var strings = input.Split(",");
            
            var indexes = new List<int>();
            foreach (var c in strings)
            {
                if (c.Equals("p") || c.Equals("f") || c.Equals("b"))
                {
                    continue;
                }               
                else
                {
                    if (!c.Equals(""))
                    {
                        var l = Int32.Parse(c);
                        indexes.Add(l);
                    }
                    
                }
            }                     
            return indexes;
        }

        /// <summary>
        /// Takes a list of indexes and seletcs the company at that index.
        /// Then removes the selected company from the _companies list.
        /// </summary>
        /// <param name="indexes">selected indexes</param>
        private void SelectCompany(List<int> indexes)
        {
            foreach (var index in indexes)
            {
                if(index <= (this._companies.Count -1) && index >= 0)
                    this._selectedCompanies.Add(this._companies.ElementAt(index));
                                
            }

            foreach (var selectedCompany in _selectedCompanies)
            {               
                for (int i = 0; i < this._companies.Count; i++)
                {                    
                    if (selectedCompany.CompanyDirectoryEntryReffNumber.Equals(_companies.ElementAt(i).CompanyDirectoryEntryReffNumber))
                    {
                        this._companies.RemoveAt(i);
                    }
                }               
            }
        }

        /// <summary>
        /// Prints the companies in the _selectedCompanies list
        /// </summary>
        private void PrintSelected()
        {
            var index = 0;

            Console.Clear();
            Console.WriteLine("Companies:");
            foreach (var item in _selectedCompanies)
            {
                Console.WriteLine($"( {index} )  " + item.CompanyName);
                Console.WriteLine("             Country: " + item.Country);
                Console.WriteLine("             Address: " + item.Address);
                Console.WriteLine("             Website: " + item.Website);
                Console.WriteLine("             Latitude: " + item.Latitude);
                Console.WriteLine("             Longitude: " + item.Longitude);
                Console.WriteLine("             Created: " + item.Created);
                Console.WriteLine("             ReffNumber: " + item.CompanyDirectoryEntryReffNumber);
                Console.WriteLine("             Status: " + item.Status);
                index++;
            }
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to continiue.");
            Console.ReadLine();
        }

        /// <summary>
        /// Print the related dialog for this menu.
        /// </summary>
        private void MenuDialog()
        {
            var index = 0;

            Console.Clear();
            Console.WriteLine("Companies:");            
            foreach (var item in this._companies)
            {               
                Console.WriteLine($"( {index} )  " + item.CompanyName);
                Console.WriteLine("             Country: " + item.Country);
                Console.WriteLine("             Address: " + item.Address);
                Console.WriteLine("             Website: " + item.Website);
                Console.WriteLine("             Latitude: " + item.Latitude);
                Console.WriteLine("             Longitude: " + item.Longitude);
                Console.WriteLine("             Created: " + item.Created);
                Console.WriteLine("             ReffNumber: " + item.CompanyDirectoryEntryReffNumber);
                Console.WriteLine("             Status: " + item.Status);
                index++;
            }
            Console.WriteLine();
            Console.WriteLine("Total: " + (index) + " Includes 0.");
            Console.WriteLine("Type P to print selected.");
            Console.WriteLine("Type F to FINISH and Save.");
            Console.WriteLine("Type B to go BACK without saving.");
            Console.WriteLine("Type NUMBER to SELECT COMPANY. Seperate WITH COMMA: , .");
        }
    }
}
