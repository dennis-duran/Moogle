namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        
        //crear diccionario para almacenar similitud con la query de cada documento
        Dictionary<string, double> match = new Dictionary<string, double>();
        //crear lista con las palabras que tienen un operador
        List<string> paldif = new List<string>();
        //crear las palabras del query como vector
        List<string> vectorQUERY = new List<string>();
        //Normalizar la query
        query = query.Replace("í", "i").Replace("á", "a").Replace("é", "e").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u");
        List<string> QueryARRAY = MakeSearch.NormalizeQuery(query);
        //crear la sugerencia como apoyo para la busqueda usando la distancia de Hamming
        string suggestion = "";
        for (int i = 0; i < QueryARRAY.Count; i++)
        {
            if (!InfoDocs.Datos.ContainsKey(QueryARRAY[i]))
            {
                int distance = int.MaxValue;
                string Word="";
                foreach(string options in InfoDocs.Datos.Keys)
                {
                    if (MakeSearch.H_Distance(QueryARRAY[i], options) <= distance)
                    {
                        distance = MakeSearch.H_Distance(QueryARRAY[i], options);
                        Word = options;
                    }
                }
                suggestion = suggestion + " " + Word;
                

            }
            else suggestion =suggestion+ " "+ QueryARRAY[i];

        }
        //Rellenar el vector del query y la lista de palabras con operadores
        MakeSearch.AnalyzeQuery(QueryARRAY,paldif,vectorQUERY);
        //crear array con tf * idf de las palabras del query como vector 
        double[] Querytfidf = MakeSearch.DataVector(vectorQUERY, QueryARRAY);

        //identificar que el resultado de busqueda no es vacio
        int relevante = 0;
        for(int i = 0; i < Querytfidf.Length; i++)
        {
            if(Querytfidf[i]!=0) relevante++;
        }
        if (relevante == 0)
        {
            SearchItem[] item = new SearchItem[1] {new SearchItem("No Se Encontraron Coincidencias","Pruebe introducir otro parametro de busqueda",0)};
            return new SearchResult(item, suggestion);
        }
        //Modificar valores de tf * idf segun el operador de importancia
        MakeSearch.ImportantOperator(paldif, Querytfidf,vectorQUERY);
        //calcular magnitud del vector de la query
        double MagnitudeQuery = MakeSearch.MagnitudeQuery(Querytfidf);
        //calcular la similitud del query con cada documento y agregarla a su diccionario correspondiente
        for (int recorredatos = 1; recorredatos <= InfoDocs.ListadeListas.Count; recorredatos++)
        {
            double[] datosvector= MakeSearch.CreateVectorDoc(vectorQUERY,recorredatos);
            double sumapuntos= MakeSearch.Sumapunto(Querytfidf,datosvector);
            double magnitudes = MakeSearch.MagnitudeDoc(recorredatos);
            double similarity = MakeSearch.Similarity(sumapuntos, magnitudes, MagnitudeQuery);
            if(similarity!=0) match.Add(InfoDocs.names[recorredatos - 1], similarity);
        }
        //aplicar cambios en el diccionario en dependencia de los operadores
        MakeSearch.Operators(paldif,InfoDocs.ListadeListas,match,InfoDocs.DatosCercania);
        //comprobar que el resultado de busqueda no sea vacio
        if (match.Count == 0)
        {

            SearchItem[] item = new SearchItem[1] { new SearchItem("No Se Encontraron Coincidencias", "Pruebe introducir otro parametro de busqueda", 0) };
            return new SearchResult(item, suggestion);
        }
        //ordenar el diccionario de las similitudes en orden descendente
        var OrderMatch = from entry in match orderby entry.Value descending select entry;
        //crear lista de palabras mas importantes para el snippet
        List<string> MIWs=MakeSearch.MostImportantWord(vectorQUERY,Querytfidf);
        //crear lista con los nombres de los documentos ordenados
        List<string> OrderNames = new List<string>();
        //limitar cantidad de resultados a 5
        int n = 0;
        foreach(var name in OrderMatch)
        {
            OrderNames.Add(name.Key);
            n++;
        }
        if (n > 5) n = 5;

        //guardar posicion de los documentos segun el orden de importancia
        int[] Results_docs=new int[n];
        int pos = 0;
        foreach(string name in OrderNames)
        {
            if (pos < n)
            {
                for (int j = 0; j < InfoDocs.names.Length; j++)
                {
                    if (name == InfoDocs.names[j]) Results_docs[pos] = j;
                }
                pos++;
            }
            
        }
        //crear un diccionario con cada documento y la palabra del query mas importante, siempre que el documento contenga a la palabra
        Dictionary<string, string> Important_Asignation=MakeSearch.ImportanceAsignation(MIWs,InfoDocs.names,Results_docs);




        //devolver los resultados
        SearchItem[] items = new SearchItem[n];
        for (int i = 0; i < n; i++)
        {
            items[i] = new SearchItem(OrderNames[i], MakeSearch.Snipet(Important_Asignation[OrderNames[i]], Results_docs[i]), match[OrderNames[i]]);
        }
       
        return new SearchResult(items, suggestion);

    }

    

}

