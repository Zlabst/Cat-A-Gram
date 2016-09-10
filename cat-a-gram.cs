using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Cat_A_Gram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cat-A-Gram [Version 2.1.1]");
            Console.WriteLine("Instagram media snatcher.\n");

            if (args.Length == 0)
            {
                // The progam is launched by exe or without parameters.
                Console.WriteLine("  USER ERROR: No program parameter given. Type CAT-A-GRAM /H to see available parameters.");
                Console.Write("  Press any key to continue . . .");
                Console.ReadLine();
            }
            else
            {
                // There is at least one parameter.
                switch (args[0])
                {
                    case "/H":
                        // Display the help screen.
                        DisplayHelp();
                        break;
                    default:
                        // The "real" program.
                        string mediaNumber = args[0];
                        string mediaExtension;
                        string mediaUrl = "https://www.instagram.com/p/" + mediaNumber + "/";

                        Console.WriteLine("  Retrieving source code of " + mediaUrl + "\n");
                        WebClient webClient = new WebClient();

                        try
                        {
                            // Try downloading the source code of the page.
                            string htmlSourceCode = webClient.DownloadString(mediaUrl);

                            // Setting up some locations in the full source code.
                            int displaySourceLocation = htmlSourceCode.IndexOf("\"display_src\"");
                            int videoUrlLocation = htmlSourceCode.IndexOf("\"video_url\"");
                            int captionLocation = htmlSourceCode.IndexOf("\"caption\"");
                            int captionEndMarker;

                            string mediaUrl_Rought;
                            string mediaUrl_Direct;
                            
                            if (htmlSourceCode.Contains("mp4"))
                            {
                                mediaUrl_Rought = htmlSourceCode.Substring(videoUrlLocation, htmlSourceCode.IndexOf("\"usertags\"") - videoUrlLocation);
                                mediaUrl_Direct = mediaUrl_Rought.Substring(mediaUrl_Rought.IndexOf("http"), mediaUrl_Rought.IndexOf(".mp4") + 4 - mediaUrl_Rought.IndexOf("http"));
                                captionEndMarker = htmlSourceCode.IndexOf("\",\"");
                                mediaExtension = ".mp4";
                            }
                            else
                            {
                                mediaUrl_Rought = htmlSourceCode.Substring(displaySourceLocation, htmlSourceCode.IndexOf("\"location\"") - displaySourceLocation);
                                mediaUrl_Direct = mediaUrl_Rought.Substring(mediaUrl_Rought.IndexOf("http"), mediaUrl_Rought.IndexOf("?") - mediaUrl_Rought.IndexOf("http"));
                                captionEndMarker = htmlSourceCode.IndexOf("\",\"");
                                mediaExtension = ".jpg";
                            }

                            // WARNING: ugly code commences!
                            string CaptionText_Rough = htmlSourceCode.Substring(captionLocation);
                            string CaptionText_Clean = CaptionText_Rough.Substring(CaptionText_Rough.IndexOf("\": \"") + 4).Substring(0, CaptionText_Rough.Substring(CaptionText_Rough.IndexOf("\": \"") + 4).IndexOf("\", \""));
                            string CaptionText_Emoji = DecodeNonAscii(CaptionText_Clean);

                            string fileName;
                            if (args.Length >= 2)
                            {
                                // The user specified some kind of name.
                                switch (args[1].ToLower())
                                {
                                    case "/a":
                                    case "/address":
                                    case "/code":
                                        fileName = mediaNumber + mediaExtension;
                                        break;
                                    case "/c":
                                    case "/caption":
                                        fileName = CaptionText_Emoji + mediaExtension;
                                        break;
                                    default:
                                        fileName = args[1].ToLower() + mediaExtension;
                                        break;
                                }
                            }
                            else
                            {
                                // The user did not specify a name, revert to mediaNumber.
                                fileName = mediaNumber + mediaExtension;
                            }

                            if (fileName.IndexOfAny(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }) != -1)
                            {
                                // The user wanted to input illegal character in filename.
                                Console.WriteLine("  MEDIA ERROR: The file name contains illegal characters (<, >, :, \", /, \\, |, ?, *)");
                            }
                            else
                            {
                                Console.WriteLine("  Downloading " + mediaUrl_Direct + "\n");
                                webClient.DownloadFile(mediaUrl_Direct, fileName);
                                Console.WriteLine("  File downloaded to " + fileName);
                            }
                        }
                        catch (WebException)
                        {
                            // Probably a 404 error from webClient.
                            Console.WriteLine("  NETWORK ERROR: No Instagram media found. Check if this URL points to a valid Instagram media page: " + mediaUrl);
                        }
                        break;
                }
            }
        }

        static void DisplayHelp(bool EmptyArgs = false)
        {
            Console.WriteLine("Downloads an Instagram media by a given URL code (eg. BJSzqWBhppW).\n");
            Console.WriteLine("usage: CAT-A-GRAM [code] [name] [/H]\n");
            Console.WriteLine("  [code]\tSpecifies the Instagram media to be downloaded by its URL code.\n");
            Console.WriteLine("  [name]\tSpecifies the name of the file. If no parameter given, defaults to URL code.");
            Console.WriteLine("\t\t  /A, /ADDRESS, /CODE	Saves the media as its Instagram URL code.");
            Console.WriteLine("\t\t  /C, /CAPTION		Saves the media as its Instagram caption.");
            Console.WriteLine("\t\tAnything else is interpreted as custom name (eg. \"image\").\n");
            Console.WriteLine("  /H\t\tDisplays this help screen.\n");
            Console.WriteLine("The program can identify jpg and mp4 files and saves them appropriately.\n");
            Console.WriteLine("Error codes:\n");
            Console.WriteLine("  MEDIA ERROR\tThe file name contains illegal characters, usually when it is present in captions.");
            Console.WriteLine("  NETWORK ERROR\tThe program couldn't find an Instagram page, and the server sent back a 404 error.");
            Console.WriteLine("  USER ERROR\tYou didn't give or mistyped a program parameter.");
        }

        //Thanks to Adam Sills from stackoverflow for this function.
        static string DecodeNonAscii(string value)
        {
            // This funtion correctly shows emojis in filenames and supported fonts instead of s/00fe2 or something like that.
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
    }
}
