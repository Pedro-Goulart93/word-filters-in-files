        using Tesseract;
        using Aspose.Words;
        using System.Collections.Concurrent;
        using System.Threading;
        using System.Threading.Tasks;
        using Xceed.Words.NET;
        using iTextSharp.text.pdf;
        using iTextSharp.text.pdf.parser;
        
        public static List<int> ListarCurriculosParalelo(string[] palavrasChave, List<Recrutado> recrutados = null, List<Candidato> candidatos = null, List<string> palavrasChaveSemAcento = null)
        { 

            var palavrasChaveLocal = palavrasChave;
            var palavrasChaveSemAcentoLocal = palavrasChaveSemAcento;
            List<Recrutado> recrutado = new List<Recrutado>();

            if(recrutados != null)
                recrutado = recrutados.Where(r => r.Candidato.ExperienciaProfissional.Count() <= 0 && !String.IsNullOrEmpty(r.Candidato.ArquivoCurriculo)).ToList();

            var rangePartitioner = Partitioner.Create(0, recrutado.Count());
            var lista = new ConcurrentBag<int>();

            if (recrutado.Count() > 0)
            {
                Parallel.ForEach(rangePartitioner, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        StringBuilder sb = new StringBuilder();
                        string file = HostingEnvironment.MapPath("~/Content/Upload/Curriculos/") + recrutado[i].Candidato.ArquivoCurriculo;
                        //verifica a extensão
                        string extensao = System.IO.Path.GetExtension(file);

                        //Metodo PDF
                        if (extensao == ".pdf" || extensao == ".doc" || extensao == ".odt" || extensao == ".rtf")
                        {

                            if (extensao != ".pdf")
                            {

                                try
                                {
                                    lock (locker)
                                    {
                                        Document doc = new Document(file);
                                        doc.Save(HostingEnvironment.MapPath("~/Content/Upload/Curriculos/curriculoConvert.pdf"));
                                        using (PdfReader reader = new PdfReader(HostingEnvironment.MapPath("~/Content/Upload/Curriculos/curriculoConvert.pdf")))
                                        {

                                            for (int page = 1; page <= reader.NumberOfPages; page++)
                                            {
                                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                                string text = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                                                text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                                                sb.Append(text);
                                            }
                                        }
                                    }

                                }
                                catch (Exception)
                                {
                                    sb.Clear();
                                }

                            }
                            else
                            {
                                try
                                {
                                    using (PdfReader reader = new PdfReader(file))
                                    {
                                        for (int page = 1; page <= reader.NumberOfPages; page++)
                                        {
                                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                            string text = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                                            text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                                            sb.Append(text);
                                        }
                                    }


                                }
                                catch (Exception)
                                {
                                    sb.Clear();
                                }
                            }

                        }
                        //Método Word
                        if (extensao == ".docx")
                        {
                            try
                            {
                                var doc = DocX.Load(file);

                                sb.Append(doc.Text);

                            }
                            catch (Exception)
                            {
                                sb.Clear();
                            }

                        }
                        //Método Imagem
                        if (extensao == ".jpeg" || extensao == ".jpg" || extensao == ".png" || extensao == ".bitmap")
                        {
                            try
                            {
                                using (var engine = new TesseractEngine(HostingEnvironment.MapPath("~/Content/tesslang/"), "por", EngineMode.Default))
                                {
                                    using (var img = Pix.LoadFromFile(file))
                                    {
                                        using (var page = engine.Process(img))
                                        {
                                            sb.Append(page.GetText());

                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                sb.Clear();
                            }

                        }
                        int c = 0;

                        for (int p = 0; p < palavrasChaveLocal.Count(); p++)
                        {
                            if (sb.ToString().ToLower().Contains(palavrasChaveLocal[p].ToLower().Trim()) || sb.ToString().ToLower().Contains(palavrasChaveSemAcentoLocal[p].ToLower().Trim()))
                            {
                                c++;
                                if (c > 0)
                                {
                                    lista.Add(recrutado[i].IdCandidato);
                                    break;
                                }
                            }
                        }
                        sb.Clear();
                    }                                                    
                });
            }
            else
            {
                
                Parallel.ForEach(candidatos, item =>
                {
                    StringBuilder sb = new StringBuilder();

                    string file = HostingEnvironment.MapPath("~/Content/Upload/Curriculos/") + item.ArquivoCurriculo;
                    //verifica a extensão
                    string extensao = System.IO.Path.GetExtension(file);

                    //Metodo PDF
                    if (extensao == ".pdf" || extensao == ".doc" || extensao == ".odt")
                    {
                        if (extensao != ".pdf")
                        {
                            lock (locker)
                            {
                                try
                                {
                                    Document doc = new Document(file);
                                    doc.Save(HostingEnvironment.MapPath("~/Content/Upload/Curriculos/curriculoConvert.pdf"));
                                    using (PdfReader reader = new PdfReader(HostingEnvironment.MapPath("~/Content/Upload/Curriculos/curriculoConvert.pdf")))
                                    {
                                        for (int page = 1; page <= reader.NumberOfPages; page++)
                                        {
                                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                            string text = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                                            text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                                            sb.Append(text);
                                        }
                                    }

                                }
                                catch (Exception)
                                {
                                    sb.Clear();
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                using (PdfReader reader = new PdfReader(file))
                                {
                                    for (int page = 1; page <= reader.NumberOfPages; page++)
                                    {
                                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                        string text = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                                        text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                                        sb.Append(text);
                                    }
                                }

                            }
                            catch (Exception)
                            {
                                sb.Clear();
                            }
                        }


                    }
                    //Método Word
                    if (extensao == ".docx")
                    {
                        try
                        {
                            var doc = DocX.Load(file);

                            sb.Append(doc.Text);

                        }
                        catch (Exception)
                        {
                            sb.Clear();
                        }


                    }
                    //Método Imagem
                    if (extensao == ".jpeg" || extensao == ".jpg" || extensao == ".png" || extensao == ".bitmap")
                    {
                        try
                        {
                            using (var engine = new TesseractEngine(HostingEnvironment.MapPath("~/Content/tesslang/"), "por", EngineMode.Default))
                            {
                                using (var img = Pix.LoadFromFile(file))
                                {
                                    using (var page = engine.Process(img))
                                    {
                                        sb.Append(page.GetText());

                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            sb.Clear();
                        }

                    }
                    
                    int c = 0;
                    for (int i = 0; i < palavrasChave.Count(); i++)
                    {
                        if (sb.ToString().ToLower().Contains(palavrasChave[i].ToLower().Trim()) || sb.ToString().ToLower().Contains(palavrasChaveSemAcento[i].ToLower().Trim()))
                        {
                            c++;
                            if (c > 0)
                            {
                                lista.Add(item.IdCandidato);
                                break;  
                            }

                        }
                    }
                    sb.Clear();                    
                });
            }       
            return lista.ToList();
        }
