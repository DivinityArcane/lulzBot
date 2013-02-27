﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace lulzbot
{
    public class Storage
    {
        /// <summary>
        /// Creates the storage directory if it doesn't exist.
        /// </summary>
        private static void ConfirmStorageDir ()
        {
            try
            {
                if (!Directory.Exists("./Storage"))
                {
                    Directory.CreateDirectory("./Storage");
                }
            }
            catch (Exception E)
            {
                ConIO.Write("Fatal error: Cannot create storage directory: " + E.ToString());
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Loads type T from file filename
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="filename">Storage file name</param>
        /// <returns>object of type T or null</returns>
        public static T Load<T> (String filename)
        {
            ConfirmStorageDir();

            try
            {
                String n_filename = String.Format("./Storage/{0}.sto", Regex.Replace(filename, "[^a-zA-Z0-9_]", ""));

                if (!File.Exists(n_filename))
                {
                    return default(T);
                }
                else
                {
                    String buffer = String.Empty;

                    using (Stream stream = new FileStream(n_filename, FileMode.Open))
                    {
                        using (StreamReader file = new StreamReader(stream))
                        {
                            buffer = file.ReadToEnd();
                        }
                    }

                    T obj = JsonConvert.DeserializeObject<T>(buffer);

                    return obj;
                }
            }
            catch (Exception E)
            {
                ConIO.Warning("Storage", "Error while loading file[" + filename + "]: " + E.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Saves the object to storage file filename
        /// </summary>
        /// <param name="filename">Storage file name</param>
        /// <param name="obj">Data object</param>
        public static void Save (String filename, object obj)
        {
            ConfirmStorageDir();

            filename = String.Format("./Storage/{0}.sto", Regex.Replace(filename, "/([^a-zA-Z0-9_]+)/g", ""));

            if (obj == null)
            {
                ConIO.Write("WARNING: Failed to write, object null for file: " + filename, "Storage");
                return;
            }

            String output = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            using (Stream stream = new FileStream(filename, FileMode.Create))
            {
                using (StreamWriter file = new StreamWriter(stream))
                {
                    file.Write(output);
                }
            }
        }
    }
}
