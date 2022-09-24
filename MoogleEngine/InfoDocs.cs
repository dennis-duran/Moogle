using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public static class InfoDocs
    {
        //Lista de Listas, cada lista contiene las palabras de un documento
        internal static List<List<string>>? ListadeListas;
        //Diccionario que contiene todas las palabras de los documentos y un array de double para cada palabra
        //Ese array contendra en la posicion 0 el idf de la palabra y en la posicion n el tf de la palabra en el documento n-1
        internal static Dictionary<string, double[]>? Datos;
        //Lista de diccionarios para el operador de cercania
        //Cada diccionario contendra las palabras de un documento como clave, y el valor asociado a esa clave sera otro diccionario que contendra a sus 10 palabras mas cercanas y a la distancia a la que se encuentras de la palabra clave
        internal static List<Dictionary<string, Dictionary<string, int>>>? DatosCercania;
        //Array con los nombres de los documentos
        internal static string[]? names;

        //Metodo que permite asignar los datos necesarios a las listas y diccionarios desde la ejecucion del proyecto
        public static void LoadInfo()
        {
            
            ListadeListas = ReadFiles(GetFiles(GetPath()));
            Datos= CreateD(ListadeListas);
            DatosCercania = NearWords(ListadeListas);
            names= GetNames(GetFiles(GetPath()));

        }

        //Metodo para crear un array con las rutas de cada documento 
        static string[] GetFiles(string path)
        {
            string[] direccion = Directory.GetFiles(path);
            return direccion;
        }
        //Obtemcion de ruta de los documentos
       internal static string GetPath()
       {
              string current= Directory.GetCurrentDirectory();
            return Path.Join(current, "..", "/Content");
       }
        //Saber la longitud del path para poder saber el nombre de los documentos
        internal static int PathLength(string path)
        {
            return path.Length;
        }
        //Metodo para crear el array con los nombres de los documentos
        internal static string[] GetNames(string[] paths)
        {
            string[] names = new string[paths.Length];
            for(int i=0; i<paths.Length; i++)
            {
                names[i] = paths[i].Substring(PathLength(GetPath()+1));
            }
            return names;
        }
        //Metodo para leer los documentos y crear la Lista de Listas
        internal static List<List<string>> ReadFiles(string[] paths)
        {
            List<List<string>> ListadeListas = new List<List<string>>();
            //Array con caracteres delimitadores de palabras
            char[] caracteresINNECESARIOS = new char[] { ' ', '(', '>', ')', '!', ':', ';', '"', '*', '/', '-', '+', '&', '_', '#', '@', '~', '`', '|', '•', '{', '}', ']', '[', '<', '\'', '\t', '?', '\\', '%', '^', '$', ',', '.', '»', '«', '¿', '”', '“', '¡', '—', '…' };
            for (int i = 0; i < paths.Length; i++)
            {
                //Para leer txt
                StreamReader txt = new StreamReader(paths[i]);
                string txtLEIDO = txt.ReadToEnd();
                //Eliminar saltos de linea y reemplazar tildes
                string txtSINSALTOS = txtLEIDO.Replace("\n", " ").Replace("\r", " ").Replace("í", "i").Replace("á", "a").Replace("é", "e").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u");
                //Crear array de strings usando el metodo ManejarTexto (explicado posteriormente)
                string[] txtARRAY = ManejarTexto(txtSINSALTOS, caracteresINNECESARIOS);
                //Convertir el array de strings en una lista
                List<string> Txtlista = txtARRAY.ToList();
                //Agregar lista a la lista de listas
                ListadeListas.Add(Txtlista);
            }
            return ListadeListas;
        }
        //Metodo para reconocer palabras segun delimitadores y crear array de strings
        internal static string[] ManejarTexto(string texto, char[] delimitadores)
        {
            //crear el array
            string[] txtARRAY = texto.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            //llevar cada palabra a minuscula
            for (int j = 0; j < txtARRAY.Length; j++)
            {
                txtARRAY[j] = txtARRAY[j].ToLower();
                
               
            }
            return txtARRAY;
        }
        //Metodo para crear diccionario con las palabras y su array de tf e idf
        internal static Dictionary<string,double[]> CreateD(List<List<string>> ListadeListas) 
        {
            //crear el diccionario
            Dictionary<string, double[]> DATOS = new Dictionary<string, double[]>();
            int n = 1;
            //contar cuantas veces aparece cada palabra en un documento n y guardarlo en la posicion n+1 de un array
            //contar en cuantos documentos aparece una palabra y guardarlo en la posicion 0 de un array


            foreach (List<string> lista in ListadeListas)
            {

                for (int i = 0; i < lista.Count; i++)
                {
                    if (!(DATOS.ContainsKey(lista[i])))
                    {
                        //crear el array
                        double[] datospalabra = new double[ListadeListas.Count + 1];
                        datospalabra[0] = 1;
                        datospalabra[n] = 1;
                        
                        DATOS.Add(lista[i], datospalabra);
                    }
                    else
                    {
                        
                        if (DATOS[lista[i]][n] == 0) DATOS[lista[i]][0]++;
                        DATOS[lista[i]][n]++;
                    }

                }
                n++;
            }
            //En la posicion 0 de cada array usar la formula: Idf=Math.Log2(Cantidad total de documentos / Cantidad de documentos que contienen a la palabra);
            //En la posicion n del array usar la formula: TF= Cantidad de veces que aparece una palabra / Cantidad de palabras del documento
            foreach (var palabra in DATOS.Keys)
            {
                DATOS[palabra][0] = Math.Log2(ListadeListas.Count / DATOS[palabra][0]);
                for (int i = 1; i <= ListadeListas.Count; i++)
                {
                    DATOS[palabra][i] = DATOS[palabra][i] / ListadeListas[i - 1].Count;
                }
            }
            return DATOS;
        }
        //Crear lista de diccionarios para las palabras cercanas
        internal static List<Dictionary<string, Dictionary<string, int>>> NearWords(List<List<string>> ListadeListas)
        {
            //Crear la lista
            List<Dictionary<string, Dictionary<string, int>>> cercania = new List<Dictionary<string, Dictionary<string, int>>>();
            for (int i = 0; i < ListadeListas.Count; i++)
            {
                //crear diccionario para cada documento
                Dictionary<string, Dictionary<string, int>> cerca = new Dictionary<string, Dictionary<string, int>>();
                for (int j = 0; j < ListadeListas[i].Count; j++)
                {
                    //crear diccionario para cada palabra
                    Dictionary<string, int> palabrascercanas = new Dictionary<string, int>();
                    //agregar palabras cercanas a la palabra clave
                    if (!(cerca.ContainsKey(ListadeListas[i][j])))
                    {
                        
                        try
                        {
                            for (int k = 1; k <= 11; k++)
                            {
                                if (!(palabrascercanas.ContainsKey(ListadeListas[i][j + k]))) palabrascercanas.Add(ListadeListas[i][j + k], k);
                            }
                        }
                        catch { };
                        cerca.Add(ListadeListas[i][j], palabrascercanas);

                    }
                    else
                    {
                        try
                        {
                            for (int k = 1; k <= 11; k++)
                            {
                                if (!(cerca[ListadeListas[i][j]].ContainsKey(ListadeListas[i][j + k]))) cerca[ListadeListas[i][j]].Add(ListadeListas[i][j + k], k);
                                else
                                {
                                    if (cerca[ListadeListas[i][j]][ListadeListas[i][j + k]] > k) cerca[ListadeListas[i][j]][ListadeListas[i][j + k]] = k;
                                }
                            }
                        }
                        catch { };
                    }

                }
                cercania.Add(cerca);
            }
            return cercania;
        }
    }
}
