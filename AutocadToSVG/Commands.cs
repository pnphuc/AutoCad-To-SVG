using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Thư viện Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

//Thư viện Aspose.CAD cho .NET (là API xử lý CAD file độc lập với .NETCore)
using Aspose.CAD;
using Aspose.CAD.ImageOptions;
using Aspose.CAD.ImageOptions.SvgOptionsParameters;

//Thư viện XML thực thi các lệnh cơ bản: mở, xoá
using System.Xml;

namespace AutocadToSVG
{
    public class Commands
    {   
        //Câu lệnh command in autocad để thực hiện Running class
        [CommandMethod("TOSVG")]
        //Hàm thực hiện chuyển đổi file .DWG (file mặc định trong AutoCad) sang file .SVG
        public static void ConvertDwgToSVG()
        {
            //Lấy quyền truy cập vào API AutoCad để work với bản vẽ
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            //Lấy cơ sở dữ liệu được liên kết với file đang hoạt động ở AutoCad
            Database db = doc.Database;
            
            Editor ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Lấy không gian hiện tại
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);

                // Tạo một file .DWG tạm thời
                string tempDwgFile = System.IO.Path.GetTempFileName();
                db.SaveAs(tempDwgFile, DwgVersion.Current);

                // Tải file .DWG tạm thời
                Aspose.CAD.Image image = Aspose.CAD.Image.Load(tempDwgFile);

                // Khởi tạo class object
                Aspose.CAD.ImageOptions.SvgOptions options = new Aspose.CAD.ImageOptions.SvgOptions();

                // Đặt chế độ màu SVG với Grayscale 
                options.ColorType = SvgColorMode.Grayscale;
                options.TextAsShapes = true;

                // Tạo tên tệp output .SVG
                string svgFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(doc.Name), System.IO.Path.GetFileNameWithoutExtension(doc.Name) + ".svg");

                // Lưu file .SVG
                image.Save(svgFile, options);

                //Khởi tạo XML
                XmlDocument svgDocument = new XmlDocument();
                
                //Load File SVG
                svgDocument.Load(svgFile);

                //Lấy thẻ có tags là <g>
                XmlNodeList gElements = svgDocument.GetElementsByTagName("g");
                
                for (int i = 2; i < gElements.Count; i++)
                {
                    XmlNode gElement = gElements[i];
                    //Xoá logo của Apose.Cad
                    gElement.ParentNode.RemoveChild(gElement);
                }

                //Lưu file
                svgDocument.Save(svgFile);

                // Hiển thị cho người dùng đã lưu thành công ở vị trí nào
                ed.WriteMessage("\nExported to {0}", svgFile);

                //Lưu các thay đổi vào cơ sở dữ liệu
                tr.Commit();
            }
        }
    }
}

