using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    internal class MakeSearch
    {
        //Metodo para normalizar la query y devolverla en forma de lista
        internal static List<string> NormalizeQuery(string Query)
        {
            //array de delimitadores
            char[] carINQuery = new char[] { ' ', '(', '>', ')', ':', ';', '"', '/', '-', '+', '&', '_', '#', '@', '`', '|', '•', '{', '}', ']', '[', '<', '\'', '\t', '?', '\\', '%', '$', ',', '.', '»', '«', '¿', '”', '“', '¡', '—', '…' };
            List<string> QueryARRAY = InfoDocs.ManejarTexto(Query, carINQuery).ToList();
            return QueryARRAY;
        }
        // Metodo para analizar la query
        // Crea una lista con las palabras con operadores y crea la query como vector (lista sin palabras repetidas)
        internal static void AnalyzeQuery(List<string> QueryARRAY, List<string> paldif, List<string> vectorQUERY)
        {
            int cerc = 0;
            for (int j = 0; j < QueryARRAY.Count; j++)
            {
                string mult = QueryARRAY[j];
                //para operador de aparece y no aparece
                if (QueryARRAY[j][0] == '^' || QueryARRAY[j][0] == '!')
                {
                    paldif.Add(QueryARRAY[j]);
                    QueryARRAY[j] = QueryARRAY[j].Substring(1);
                    mult = QueryARRAY[j];

                }
                //para operador de importancia
                if (QueryARRAY[j][0] == '*')
                {
                    paldif.Add(QueryARRAY[j]);
                    while (mult[0] == '*')
                    {
                        mult = mult.Substring(1);
                    }
                    QueryARRAY[j] = mult;

                }
                //para operador de cercania
                if (!(vectorQUERY.Contains(QueryARRAY[j]))) vectorQUERY.Add(QueryARRAY[j]);
                if (QueryARRAY[j] == "~")
                {
                    if (!(paldif.Contains("@" + QueryARRAY[j - 1] + "@" + QueryARRAY[j + 1]))) paldif.Add("@" + QueryARRAY[j - 1] + "@" + QueryARRAY[j + 1]);
                    cerc++;
                }
            }
            for (int j = 0; j < cerc; j++)
            {
                QueryARRAY.Remove("~");
            }
        }
        //Crea un array con el tf*idf de las palabras del query como vector
        internal static double[] DataVector(List<string> vectorQUERY, List<string> QueryARRAY)
        {
            double[] Querytfidf = new double[vectorQUERY.Count];
            int cont = 0;
            foreach (string key in vectorQUERY)
            {
                double idfquery = 0;
                if (InfoDocs.Datos.ContainsKey(key)) idfquery = InfoDocs.Datos[key][0];
                Querytfidf[cont] = TF(key, QueryARRAY) * idfquery;
                cont++;
            }
            return Querytfidf;
        }

        //Metodo para calcular tf en el query
        internal static double TF(string s, List<string> list)
        {
            double tf = 0;
            foreach (string palabra in list)
            {
                if (palabra == s) tf++;
            }
            return tf / list.Count;
        }

        //Metodo para calcular la magnitud del query com vector, usando la formula Magnitud ^2 = (vector[0])^2 + (vector[1])^2.....+(vector[n])^2
        internal static double MagnitudeQuery(double[] Querytfidf)
        {
            double magnitudQUERY = 0;
            for (int magnitud = 0; magnitud < Querytfidf.Length; magnitud++)
            {
                magnitudQUERY = magnitudQUERY + Math.Pow(Querytfidf[magnitud], 2);
            }
            magnitudQUERY = Math.Sqrt(magnitudQUERY);
            return magnitudQUERY;
        }

        //Metodo para crear un array con el tf*idf de las palabras de un documneto
        internal static double[] CreateVectorDoc(List<string> vectorQUERY, int recorredatos)
        {
            double[] datosvector = new double[vectorQUERY.Count];
            for (int datvec = 0; datvec < vectorQUERY.Count; datvec++)
            {
                double dato = 0;
                if (InfoDocs.Datos.ContainsKey(vectorQUERY[datvec])) dato = InfoDocs.Datos[vectorQUERY[datvec]][recorredatos] * InfoDocs.Datos[vectorQUERY[datvec]][0];
                datosvector[datvec] = dato;
            }
            return datosvector;
        }

        //Metodo para calcular la sumapunto entre el vector del query y el vector del documento
        //a partir de la formula Sumapunto = Sumatoria de vector_query[n] * vector_documento[n]
        internal static double Sumapunto(double[] Querytfidf, double[] datosvector)
        {
            double Sumapuntos = 0;
            for (int vectores = 0; vectores < datosvector.Length; vectores++)
            {
                Sumapuntos = Sumapuntos + datosvector[vectores] * Querytfidf[vectores];
            }
            return Sumapuntos;
        }
        //Metodo para calcular la magnitud del vector de un documento
        internal static double MagnitudeDoc(int recorredatos)
        {
            double magnitudes = 0;
            foreach (var elemento in InfoDocs.DatosCercania[recorredatos - 1])
            {
                magnitudes = magnitudes + Math.Pow(InfoDocs.Datos[elemento.Key][recorredatos] * InfoDocs.Datos[elemento.Key][0], 2);
            }
            magnitudes = Math.Sqrt(magnitudes);
            return magnitudes;
        }
        //Metodo para saber que tan parecidos son los vectores documento y query
        //usando la formula Coseno_del_angulo_entre_vectores= Sumapunto / (magnitud_documento * magnitud_query)

        internal static double Similarity(double Sumapuntos, double magnitudes, double magnitudQUERY)
        {
            double similitud = Sumapuntos / (magnitudes * magnitudQUERY);
            return similitud;
        }
        //Metodo para trabajar con los operadores
        internal static void Operators(List<string> paldif, List<List<string>> listaDeArrays, Dictionary<string, double> match, List<Dictionary<string, Dictionary<string, int>>> cercania)
        {
            for (int operadores = 0; operadores < paldif.Count; operadores++)
            {
                string palabra = paldif[operadores].Substring(1);
                //Para operador de NO APARECE
                if (paldif[operadores][0] == '!')
                {
                    for (int i = 0; i < listaDeArrays.Count; i++)
                    {
                        if (listaDeArrays[i].Contains(palabra))
                        {
                            match.Remove(InfoDocs.names[i]);
                        }
                    }
                }
                //Para operador de APARECE
                if (paldif[operadores][0] == '^')
                {
                    for (int i = 0; i < listaDeArrays.Count; i++)
                    {
                        if (!(listaDeArrays[i].Contains(palabra)))
                        {
                            match.Remove(InfoDocs.names[i]);
                        }
                    }
                }
               //Para operador de cercania
               //ver si las palabras estan cerca segun el la lista de palabras cercanas y multiplicar de forma inversamente proporcional con la distancia entre las palabras
                if (paldif[operadores][0] == '@')
                {
                    string[] palcer = paldif[operadores].Split('@', StringSplitOptions.RemoveEmptyEntries);
                    while (palcer[1][0] == '^' || palcer[1][0] == '*' || palcer[1][0] == '~')
                    {
                        palcer[1] = palcer[1].Substring(1);
                    }
                    for (int i = 0; i < InfoDocs.names.Length; i++)
                    {
                        if (match.ContainsKey(InfoDocs.names[i]))
                        {
                            double primseg = 0;
                            if (cercania[i].ContainsKey(palcer[0]) && cercania[i][palcer[0]].ContainsKey(palcer[1]))
                            {
                                primseg = cercania[i][palcer[0]][palcer[1]];
                            }
                            double segprim = 0;
                            if (cercania[i].ContainsKey(palcer[1]) && cercania[i][palcer[1]].ContainsKey(palcer[0]))
                            {
                                segprim = cercania[i][palcer[1]][palcer[0]];
                            }
                            if (primseg != 0 || segprim != 0)
                            {

                                if (Math.Min(primseg, segprim) > 0)
                                {
                                    match[InfoDocs.names[i]] = match[InfoDocs.names[i]] * (1 + 1 / Math.Min(primseg, segprim));
                                }
                                else
                                {
                                    match[InfoDocs.names[i]] = match[InfoDocs.names[i]] * (1 + (1 / Math.Max(primseg, segprim)));

                                }
                            }
                        }
                    }



                }


            }
        }
        //Metodo para crear el snippet a partir de una palabra
        internal static string Snipet(string palabra, int doc)
        {
            if (palabra == "") return palabra;
            string pal = "";
            foreach (string str in InfoDocs.DatosCercania[doc][palabra].Keys)
            {
                if (InfoDocs.DatosCercania[doc][palabra][str] == 1) pal = pal + "..." + palabra + " " + str;
                else pal = pal + " " + str;
            }
            if (pal.Length < 146) return pal;
            else return pal.Substring(0, 147);
        }
        //Metodo para identificar las palabras mas importantes del query segun su tf * idf
        internal static List<string> MostImportantWord(List<string> vector,double[] Querytfidf)
        {
            
            string MIW = "";
            int MIP = 0;
            double[] modify = Querytfidf;
            List<string> Importancia=new List<string>();
            for(int j = 0; j < vector.Count; j++)
            {
                double MIV = -1;

                for (int i = 0; i < vector.Count; i++)
                {
                    if(modify[i] > MIV)
                    {
                        MIV = Querytfidf[i];
                        MIW= vector[i];
                        MIP = i;
                    }
                }
                 Importancia.Add(MIW);
                 modify[MIP] = -50;

            }
            return Importancia;
            

        }
        //Metodo para asignar a cada documento la palabra del query con mas importancia , siempre que el documento contenga a dicha palabra
        internal static Dictionary<string, string> ImportanceAsignation( List<string> MiwOrder, string[] names, int[]positions)
        {
            Dictionary<string, string> ImportantAsignation = new Dictionary<string, string>();
            for (int i = 0;i < positions.Length; i++)
            {
                int pal = 0;
                while (true)
                {
                    if (!InfoDocs.ListadeListas[positions[i]].Contains(MiwOrder[pal])) pal++;
                    else
                    {
                        ImportantAsignation.Add(names[positions[i]], MiwOrder[pal]);
                        break;
                    }
                }

            }
            return ImportantAsignation;









        }
        //Para el operador de importancia
        internal static void ImportantOperator(List<string> paldif, double[] Querytfidf, List<string> vector)
        {
            for (int i = 0; i < paldif.Count; i++)
            {
                if (paldif[i][0] == '*')
                {
                    string pal = paldif[i];
                    int cantidad = 0;
                    while (pal[0] == '*')
                    {
                        pal = pal.Substring(1);
                        cantidad++;

                    }
                    int pos = 0;
                    for (int j = 0; j < vector.Count; j++)
                    {
                        if (vector[j] == pal) pos = j;
                    }
                    Querytfidf[pos] = Querytfidf[pos] * (1 + cantidad);
                }
            }
        }

        //Metodo para saber la distancia entre 2 palabras, usando la formula de distancia de Hamming
        internal static int H_Distance(string main,string sec)
        {
            int min = Math.Min(main.Length, sec.Length);
            int distance=0;
            for(int i = 0; i < min; i++)
            {
                if(!(main[i] == sec[i])) distance++;
            }
            distance = distance + Math.Abs(main.Length - sec.Length);
            return distance;

        }
    }

}
