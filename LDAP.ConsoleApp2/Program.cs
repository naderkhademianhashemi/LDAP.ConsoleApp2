using Newtonsoft.Json;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Principal;

class Program
{
    static void Main(string[] args)
    {
        try
        {

            Console.WriteLine("Enter Domain Name:");
            var domain = Console.ReadLine();

            Console.WriteLine("Enter Group Name:");
            var groupname = Console.ReadLine();

            var se = !string.IsNullOrEmpty(domain) ? domain : "LDAP://192.168.20.1";


          var members=  GetUserOfGroup(groupname, se ?? "LDAP://localhost");

            if (members != null)
            {
                Console.WriteLine("Count: "+members.Count());
                File.WriteAllText("members.json", JsonConvert.SerializeObject(members
                    .Select(item=>new { item.Name , 
                        item.DisplayName, item.UserPrincipalName  ,
                        item.SamAccountName ,SID=item.Sid.Value,
                    item.StructuralObjectClass,
                    item.Description,
                    item.DistinguishedName})
                    ), System.Text.Encoding.UTF8);
                foreach (var item in members)
                {
                    Console.WriteLine($@"{item.Name},{item.DisplayName},{item.UserPrincipalName}");
                }
            }
            else
            {
                Console.WriteLine("No Member Found!");
            }
            //GetUsersOfGroup2(se);
            Main(args);
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }


        Console.ReadKey();
    }

    private static void GetUsersOfGroup2(string se)
    {
        DirectoryEntry de = new DirectoryEntry(se ?? "LDAP://localhost");
        //DirectoryEntry de = new DirectoryEntry(se);
        DirectorySearcher searcher = new DirectorySearcher(de);
        searcher.Filter = "(&(ObjectClass=User))";
        searcher.PropertiesToLoad.Add("distinguishedName");
        searcher.PropertiesToLoad.Add("sAMAccountName");
        searcher.PropertiesToLoad.Add("name");
        searcher.PropertiesToLoad.Add("objectSid");
        SearchResultCollection results = searcher.FindAll();
        int i = 1;
        foreach (SearchResult res in results)
        {
            Console.WriteLine("Result" + Convert.ToString(i++));
            DisplayProperties("distinguishedName", res);
            DisplayProperties("sAMAccouontName", res);
            DisplayProperties("name", res);
            DisplayProperties("objectSid", res);
            Console.WriteLine();
        }
    }

    private static PrincipalSearchResult<Principal> GetUserOfGroup(string groupName, string domainName)
    {
        groupName = string.IsNullOrEmpty(groupName)? null:groupName;
        var context = new PrincipalContext(ContextType.Domain, domainName);
        File.WriteAllText("context.json",JsonConvert.SerializeObject(context),System.Text.Encoding.UTF8);
        //var groupname = File.ReadAllText("group.txt", System.Text.Encoding.UTF8);
        var groupPrincipal = new GroupPrincipal(context, groupName);
 
        using (var searcher = new PrincipalSearcher(groupPrincipal))
        {
            var groups = searcher.FindAll();
            Console.WriteLine( groups.Count());
            File.WriteAllText("groups.json",
                JsonConvert.SerializeObject(groups.OfType<GroupPrincipal>().Select(k=>new { k.Name,k.DisplayName,k.UserPrincipalName,k.SamAccountName, membres= k.Members.Select(m => new {m.UserPrincipalName,m.Name,m.DisplayName,m.SamAccountName }) })), 
                System.Text.Encoding.UTF8);

            var group = groups.FirstOrDefault() as GroupPrincipal;
            return group?.GetMembers();
        }

        
    }

    private static void DisplayProperties(string property, SearchResult res)
    {
        System.IO.File.AppendAllText("rr.txt", "\n" + property, System.Text.Encoding.UTF8);
        ResultPropertyValueCollection col = res.Properties[property];
        foreach (object o in col)
        {
            System.IO.File.AppendAllText("rr.txt", "\n" + o.ToString(), System.Text.Encoding.UTF8);
        }
    }
}