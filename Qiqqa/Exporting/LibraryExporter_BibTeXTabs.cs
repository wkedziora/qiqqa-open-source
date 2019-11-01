﻿using System;
using System.Collections.Generic;
using System.Text;
using Qiqqa.DocumentLibrary;
using Qiqqa.Documents.PDF;
using Utilities;
using Utilities.BibTex.Parsing;
using Utilities.Misc;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;


namespace Qiqqa.Exporting
{
    internal class LibraryExporter_BibTeXTabs
    {
        internal static void Export(Library library, List<PDFDocument> pdf_documents, string base_path, Dictionary<string, PDFDocumentExportItem> pdf_document_export_items)
        {
            Logging.Info("Exporting entries to BibTeXTAB separated");

            // First work out what fields are available
            List<string> field_names = null;
            {
                HashSet<string> field_names_set = new HashSet<string>();
                for (int i = 0; i < pdf_documents.Count; ++i)
                {
                    PDFDocument pdf_document = pdf_documents[i];
                    if (!pdf_document.BibTex.IsEmpty())
                    {
                        BibTexItem item = pdf_document.BibTex;

                        foreach (var field in item.Fields)
                        {
                            field_names_set.Add(field.Key.ToLower());
                        }
                    }
                }

                field_names = new List<string>(field_names_set);
                field_names.Sort();
            }

            // Write out the header
            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("% -------------------------------------------------------------------------");
            sb.AppendLine(String.Format("% This tab separated file was generated by Qiqqa ({0}?ref=EXPTAB)", Common.Configuration.WebsiteAccess.Url_Documentation4Qiqqa));
            sb.AppendLine(String.Format("% {0} {1}", now.ToLongDateString(), now.ToLongTimeString()));
            sb.AppendLine("% Version 1");
            sb.AppendLine("% -------------------------------------------------------------------------");
            sb.AppendLine();

            // Headers
            sb.AppendFormat("{0}\t", "Fingerprint");
            sb.AppendFormat("{0}\t", "Filename");
            sb.AppendFormat("{0}\t", "BibTexKey");
            sb.AppendFormat("{0}\t", "BibTexType");
            foreach (string field_name in field_names)
            {
                sb.AppendFormat("{0}\t", FormatFreeText(field_name));
            }
            sb.AppendLine();

            // Write out the entries
            for (int i = 0; i < pdf_documents.Count; ++i)
            {
                StatusManager.Instance.UpdateStatus("TabExport", String.Format("Exporting entry {0} of {1}", i, pdf_documents.Count), i, pdf_documents.Count);

                PDFDocument pdf_document = pdf_documents[i];
                sb.AppendFormat("{0}\t", pdf_document.Fingerprint);
                sb.AppendFormat("{0}\t", pdf_document_export_items.ContainsKey(pdf_document.Fingerprint) ? pdf_document_export_items[pdf_document.Fingerprint].filename : "");

                if (!pdf_document.BibTex.IsEmpty())
                {
                    BibTexItem item = pdf_document.BibTex;

                    sb.AppendFormat("{0}\t", item.Key);
                    sb.AppendFormat("{0}\t", item.Type);
                    foreach (string field_name in field_names)
                    {
                        sb.AppendFormat("{0}\t", item.ContainsField(field_name) ? FormatFreeText(item[field_name]) : "");
                    }
                }

                sb.AppendLine();
            }

            // Write to disk
            string filename = Path.GetFullPath(Path.Combine(base_path, @"Qiqqa.BibTeX.tab"));
            File.WriteAllText(filename, sb.ToString());

            StatusManager.Instance.UpdateStatus("TabExport", String.Format("Exported your BibTeX tab entries to {0}", filename));
        }

        private static object FormatFreeText(string p)
        {
            if (String.IsNullOrEmpty(p))
            {
                return "";
            }

            return p.Replace("\t", "    ").Replace("\r\n", "    ").Replace("\n", "    ").Replace("\r", "    ");
        }

        private static string FormatDate(DateTime? date_time_nullable)
        {
            if (date_time_nullable.HasValue)
            {
                return date_time_nullable.Value.ToLongDateString();
            }
            else
            {
                return "";
            }
        }
    }
}

