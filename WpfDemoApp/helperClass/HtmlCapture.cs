using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDemoApp.helperClass
{
    public class HtmlCapture
    {
        private System.Windows.Forms.WebBrowser web;
        private System.Windows.Forms.Timer tready;
        private System.Drawing.Rectangle screen;
        private System.Drawing.Size? imgsize = null;

        //an event that triggers when the html document is captured
        public delegate void HtmlCaptureEvent(object sender, Uri url);

        public event HtmlCaptureEvent HtmlImageCapture;

        string fileName = "";

        //class constructor
        public HtmlCapture(string fileName)
        {
            this.fileName = fileName;

            //initialise the webbrowser and the timer
            web = new System.Windows.Forms.WebBrowser();
            tready = new System.Windows.Forms.Timer();
            tready.Interval = 2000;
            screen = Screen.PrimaryScreen.Bounds;
            //set the webbrowser width and hight
            web.Width = 1024; //screen.Width;
            web.Height = 768; // screen.Height;
                              //suppress script errors and hide scroll bars
            web.ScriptErrorsSuppressed = true;
            web.ScrollBarsEnabled = false;
            //attached events
            web.Navigating += web_Navigating;
            web.DocumentCompleted += web_DocumentCompleted;
            tready.Tick += tready_Tick;
        }


        public void Create(string url)
        {
            imgsize = null;
            web.Navigate(url);
        }

        public void Create(string url, System.Drawing.Size imgsz)
        {
            this.imgsize = imgsz;
            web.Navigate(url);
        }



        void web_DocumentCompleted(object sender,
                 WebBrowserDocumentCompletedEventArgs e)
        {
            //start the timer
            tready.Start();
        }

        void web_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //stop the timer   
            tready.Stop();
        }



        void tready_Tick(object sender, EventArgs e)
        {
            try
            {
                //stop the timer
                tready.Stop();

                mshtml.IHTMLDocument2 docs2 = (mshtml.IHTMLDocument2)web.Document.DomDocument;
                mshtml.IHTMLDocument3 docs3 = (mshtml.IHTMLDocument3)web.Document.DomDocument;
                mshtml.IHTMLElement2 body2 = (mshtml.IHTMLElement2)docs2.body;
                mshtml.IHTMLElement2 root2 = (mshtml.IHTMLElement2)docs3.documentElement;

                // Determine dimensions for the image; we could add minWidth here
                // to ensure that we get closer to the minimal width (the width
                // computed might be a few pixels less than what we want).
                int width = Math.Max(body2.scrollWidth, root2.scrollWidth);
                int height = Math.Max(root2.scrollHeight, body2.scrollHeight);

                //get the size of the document's body
                System.Drawing.Rectangle docRectangle = new System.Drawing.Rectangle(0, 0, width, height);

                web.Width = docRectangle.Width;
                web.Height = docRectangle.Height;

                //if the imgsize is null, the size of the image will 
                //be the same as the size of webbrowser object
                //otherwise  set the image size to imgsize
                System.Drawing.Rectangle imgRectangle;
                if (imgsize == null) imgRectangle = docRectangle;
                else imgRectangle = new System.Drawing.Rectangle() { Location = new System.Drawing.Point(0, 0), Size = imgsize.Value };

                //create a bitmap object 
                Bitmap bitmap = new Bitmap(imgRectangle.Width, imgRectangle.Height);
                //get the viewobject of the WebBrowser
                IViewObject ivo = web.Document.DomDocument as IViewObject;

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    //get the handle to the device context and draw
                    IntPtr hdc = g.GetHdc();
                    ivo.Draw(1, -1, IntPtr.Zero, IntPtr.Zero,
                             IntPtr.Zero, hdc, ref imgRectangle,
                             ref docRectangle, IntPtr.Zero, 0);
                    g.ReleaseHdc(hdc);
                }
                //invoke the HtmlImageCapture event
                bitmap.Save(fileName);
                bitmap.Dispose();
            }
            catch
            {
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            if (HtmlImageCapture != null) HtmlImageCapture(this, web.Url);
        }
    }

    [ComVisible(true), ComImport()]
    [GuidAttribute("0000010d-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IViewObject
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Draw(
            [MarshalAs(UnmanagedType.U4)] UInt32 dwDrawAspect,
            int lindex,
            IntPtr pvAspect,
            [In] IntPtr ptd,
            IntPtr hdcTargetDev,
            IntPtr hdcDraw,
            [MarshalAs(UnmanagedType.Struct)] ref System.Drawing.Rectangle lprcBounds,
            [MarshalAs(UnmanagedType.Struct)] ref System.Drawing.Rectangle lprcWBounds,
            IntPtr pfnContinue,
            [MarshalAs(UnmanagedType.U4)] UInt32 dwContinue);
        [PreserveSig]
        int GetColorSet([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
           int lindex, IntPtr pvAspect, [In] IntPtr ptd,
            IntPtr hicTargetDev, [Out] IntPtr ppColorSet);
        [PreserveSig]
        int Freeze([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
                        int lindex, IntPtr pvAspect, [Out] IntPtr pdwFreeze);
        [PreserveSig]
        int Unfreeze([In, MarshalAs(UnmanagedType.U4)] int dwFreeze);
    }
}
