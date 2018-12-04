﻿using System;
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

        public ResultObject RunMenu()
        {
            var badresult = new ResultObject();
            var succesresult = new ResultObject();

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
                succesresult.Payload = _selectedCompanies;
                return succesresult;
            }
            else
            {
                succesresult.IsSuccesFull = true;
                succesresult.Payload = new List<APICompany>();
                return succesresult;
            }                      
        }

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


        private void SelectCompany(List<int> indexes)
        {
            foreach (var index in indexes)
            {
                if(index <= this._companies.Count && index >= 0)
                    this._selectedCompanies.Add(this._companies.ElementAt(index));               
            }

            foreach (var selectedCompany in _selectedCompanies)
            {
                var key = selectedCompany.CompanyName + selectedCompany.Country;

                for (int i = 0; i < this._companies.Count; i++)
                {
                    var companykey = this._companies.ElementAt(i).CompanyName + this._companies.ElementAt(i).Country;

                    if (key.Equals(companykey))
                    {
                        this._companies.RemoveAt(i);
                    }
                }               
            }
        }

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
                index++;
            }
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to continiue.");
            Console.ReadLine();
        }

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
                index++;
            }
            Console.WriteLine();
            Console.WriteLine("Total: " + (index- 1));
            Console.WriteLine("Type P to print selected.");
            Console.WriteLine("Type F to FINISH and Save.");
            Console.WriteLine("Type B to go BACK without saving.");
            Console.WriteLine("Type NUMBER to SELECT COMPANY. Seperate WITH COMMA: , .");
        }
    }
}
