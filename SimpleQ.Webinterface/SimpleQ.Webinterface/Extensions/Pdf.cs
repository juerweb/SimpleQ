
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Pdf
    {
        public static bool CreateBill(ref MemoryStream stream, Bill bill, Survey[] surveys, string imgPath, DateTime lastBillDate)
        {
            try
            {
                var doc = new Document();
                doc.Info.Title = "SimpleQ Bill";
                doc.Info.Subject = "Auto generated bill document";
                doc.Info.Author = "SimpleQ";

                var section = doc.AddSection();
                section.PageSetup = doc.DefaultPageSetup.Clone();

                var image = section.AddParagraph();
                image.Format.Alignment = ParagraphAlignment.Right;
                var img = image.AddImage(imgPath);
                img.ScaleWidth = 0.1;
                img.ScaleHeight = 0.1;

                var title = section.AddParagraph();
                title.Format.Font.Size = 20;
                title.Format.Font.Color = new Color(0, 108, 202);
                title.Format.Alignment = ParagraphAlignment.Center;
                title.AddText("Bill");
                title.AddLineBreak();

                var content = section.AddParagraph();
                content.AddFormattedText("Date", TextFormat.Bold);
                content.AddText($": {bill.BillDate.ToString("yyyy-MM-dd")}");
                content.AddLineBreak();

                content.AddFormattedText("Customer code", TextFormat.Bold);
                content.AddText($": {bill.CustCode}");
                content.AddLineBreak();

                content.AddFormattedText("Bill ID", TextFormat.Bold);
                content.AddText($": {bill.BillId}");
                content.AddLineBreak();
                content.AddLineBreak();

                var table = section.AddTable();
                table.Format.Font.Size = 8;
                var width = section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

                table.AddColumn().Width = width * 0.4f; // Survey
                table.AddColumn().Width = width * 0.3f; // Period
                table.AddColumn().Width = width * 0.1f; // Number of answers
                table.AddColumn().Width = width * 0.1f; // Price per click
                table.AddColumn().Width = width * 0.1f; // Survey price

                var header = table.AddRow();
                header.HeadingFormat = true;
                header.VerticalAlignment = VerticalAlignment.Center;
                header[0].AddParagraph().AddFormattedText("Survey", TextFormat.Bold);
                header[1].AddParagraph().AddFormattedText("Period", TextFormat.Bold);
                header[2].AddParagraph().AddFormattedText("Number of answers", TextFormat.Bold);
                header[3].AddParagraph().AddFormattedText("Price per click", TextFormat.Bold);
                header[4].AddParagraph().AddFormattedText("Survey price", TextFormat.Bold);

                table.SetEdge(0, header.Index, 5, 1, Edge.Bottom, BorderStyle.Single, 0.75);

                var sum = 0m;
                var i = 0;
                foreach (var svy in surveys)
                {
                    var row = table.AddRow();
                    row.Shading.Color = i % 2 == 0 ? new Color(135, 200, 255) : new Color(172, 217, 255);
                    row.TopPadding = 2;
                    row.BottomPadding = 2;
                    row.VerticalAlignment = VerticalAlignment.Center;
                    row[0].AddParagraph(svy.SvyText);

                    var startDate = svy.StartDate <= lastBillDate ? lastBillDate : svy.StartDate;
                    if (svy.EndDate <= bill.BillDate)
                        row[1].AddParagraph($"{startDate.ToString("yyyy-MM-dd")} to {svy.EndDate.ToString("yyyy-MM-dd")}");
                    else
                        row[1].AddParagraph($"{startDate.ToString("yyyy-MM-dd")} to present");

                    var amount = svy.AnswerOptions.SelectMany(a => a.Votes).Distinct().Where(v => v.VoteDate > lastBillDate).Count();
                    row[2].AddParagraph($"{amount}");
                    row[3].AddParagraph($"€ {Math.Round(svy.PricePerClick.Value, 2)}");
                    row[4].AddParagraph($"€ {Math.Round((svy.PricePerClick * amount).Value, 2)}");

                    sum += (svy.PricePerClick * amount).Value;
                    i++;
                }

                if (sum != bill.BillPrice)
                {
                    Debug.WriteLine($"!!!:{sum} == {bill.BillPrice}");
                    // logging WARN
                }

                var total = table.AddRow();
                table.SetEdge(0, total.Index, 5, 1, Edge.Top, BorderStyle.Single, 0.75);
                total.TopPadding = 2;
                total.BottomPadding = 2;
                total.VerticalAlignment = VerticalAlignment.Center;

                total[0].MergeRight = 3;
                total[0].AddParagraph().AddFormattedText("Total cost", TextFormat.Bold);
                total[4].AddParagraph().AddFormattedText($"€ {Math.Round(bill.BillPrice, 2)}", TextFormat.Bold);

                var footer = section.AddParagraph();
                footer.AddLineBreak();
                footer.AddLineBreak();
                footer.AddText("Sincerely");
                footer.AddLineBreak();
                footer.AddText("Your SimpleQ-Team");

                var renderer = new PdfDocumentRenderer(false, PdfFontEmbedding.Always)
                {
                    Document = doc
                };
                renderer.RenderDocument();
                renderer.PdfDocument.Save(stream, false);
                renderer.PdfDocument.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Logging!
                return false;
            }
        }
    }
}