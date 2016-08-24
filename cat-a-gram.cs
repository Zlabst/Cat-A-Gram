using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Cat_A_Gram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cat-A-Gram [Version 1.0]");
            Console.WriteLine("Instagram image snatcher.\n");

            if (args.Length == 0)
            {
                //The program started without command-line arguments, probably directly from the exe.
                DisplayHelp(true);
            }
            else
            {
                if (args[0] == "help")
                {
                    DisplayHelp();
                }
                else
                {
                    //We expect the image address, not "help" or something
                    string imageAddress = args[0];
                    string completePageURL = "https://www.instagram.com/p/" + imageAddress + "/";

                    //Downloading the page's source code
                    Console.WriteLine("  Retrieving URL: " + completePageURL + "\n");
                    WebClient webClient = new WebClient();

                    try
                    {
                        //Point-Of-Connection to the internet here
                        string htmlCode = webClient.DownloadString(completePageURL); //This can crash -- 404 WebException if it can't find a page.

                        //Getting image path and cleaning it up... There is a better way sure, but fuck it, it works.
                        string imageHtmlCode = htmlCode.Substring(htmlCode.IndexOf("\"display_src\""), htmlCode.IndexOf("location") - htmlCode.IndexOf("display_src"));
                        string cleanImageURL = imageHtmlCode.Substring(imageHtmlCode.IndexOf("http"), imageHtmlCode.IndexOf("?") - imageHtmlCode.IndexOf("http"));
                        Console.WriteLine("  Image URL: " + cleanImageURL);

                        //Same thing, getting the image's caption
                        string captionHtmlCode = htmlCode.Substring(htmlCode.IndexOf("\"caption\""), htmlCode.IndexOf("\"likes\"") - htmlCode.IndexOf("\"caption\""));
                        string cleanCaptionText = captionHtmlCode.Substring(captionHtmlCode.IndexOf(":") + 3).Substring(0, captionHtmlCode.Substring(captionHtmlCode.IndexOf(":") + 3).Length - 3);
                        Console.WriteLine("  Image caption: " + cleanCaptionText);

                        string imageFilePath;

                        if (args.Length == 2)
                        {
                            switch (args[1])
                            {
                                case "a":
                                case "address":
                                    imageFilePath = imageAddress + ".jpg";
                                    //Point-Of-Connection to the internet here
                                    webClient.DownloadFile(cleanImageURL, imageFilePath); //These shouldn't throw 404s because there must be an image.
                                    Console.WriteLine("  File downloaded to " + imageFilePath);
                                    break;
                                case "c":
                                case "caption":
                                    imageFilePath = cleanCaptionText + ".jpg";
                                    //Point-Of-Connection to the internet here
                                    webClient.DownloadFile(cleanImageURL, DecodeEncodedNonAsciiCharacters(imageFilePath)); //These shouldn't throw 404s because there must be an image.
                                    Console.WriteLine("  File downloaded to " + imageFilePath);
                                    break;
                                default:
                                    Console.WriteLine("\nUSER ERROR: " + args[1] + " is not recognized as a valid [name] argument. Type \"cat-a-gram help\" for a list of arguments.");
                                    break;
                            }
                        }
                        else
                        {
                            imageFilePath = imageAddress + ".jpg";
                            //Point-Of-Connection to the internet here
                            webClient.DownloadFile(cleanImageURL, imageFilePath); //These shouldn't throw 404s because there must be an image.
                            Console.WriteLine("  File downloaded to " + imageFilePath);
                        }
                    }
                    catch (WebException)
                    {
                        //Possibly 404 caught
                        Console.WriteLine("NETWORK ERROR: No Instagram page under " + completePageURL + " is not found.");
                    }
                }
            }
        }

        static void DisplayHelp(bool EmptyArgs = false)
        {
            Console.WriteLine("Downloads an image from Instagram to the program's directory.\n");
            Console.WriteLine("cat-a-gram [code] [name]\n");
            Console.WriteLine("  [code]\tSpecifies the Instagram image by the browser address.");
            Console.WriteLine("\t\t  Example code: BJdgjueAS64");
            Console.WriteLine("  [name]\tSpecifies whether use the address or caption as the file's name.");
            Console.WriteLine("\t\t  Valid names:");
            Console.WriteLine("\t\t    a, address (default)");
            Console.WriteLine("\t\t    c, caption");
            Console.WriteLine("Example: cat-a-gram BJdgjueAS64 caption");

            if (EmptyArgs)
            {
                Console.WriteLine("Press enter to close . . .");
                Console.ReadLine();
            }
        }

        //Thanks to Adam Sills from stackoverflow for this function.
        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
    }
}
