// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

// ----------------------------------------------------------------------------------
// This code has been modified by Richard T. Broida
// Copyright 2013 Richard T. Broida
// ----------------------------------------------------------------------------------

using System;
using System.Net;
using GuestBook_WebApp;

namespace GuestBook_WebRole
{
    public partial class _Default : System.Web.UI.Page
    {
        private static bool storageInitialized = false;
        private static readonly object gate = new object();

        private void initializeStorage()
        {
            if (storageInitialized) return;

            lock (gate)
            {
                if (storageInitialized) return;

                try
                {
                    GuestBook_Controller.initializeStorage();
                }
                catch (WebException)
                {
                    throw new WebException("Storage services initialization failure. "
                       + "Check your storage account configuration settings. If running locally, "
                       + "ensure that the Development Storage service is running.");
                }
                storageInitialized = true;
            }
        }

        protected
            void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Timer1.Enabled = true;
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            DataList1.DataBind();
        }

        protected void SignButton_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                initializeStorage();

                GuestBook_Controller.createGuestBookEntry
                    (
                          guestName: NameTextBox.Text
                        , message: MessageTextBox.Text
                        , fileName: FileUpload1.FileName
                        , fileContentType: FileUpload1.PostedFile.ContentType
                        , fileStream: FileUpload1.FileContent
                    );

                NameTextBox.Text = string.Empty;
                MessageTextBox.Text = string.Empty;
                DataList1.DataBind();
            }
        }


    }
}
