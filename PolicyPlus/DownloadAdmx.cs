﻿using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolicyPlus
{
    public partial class DownloadAdmx
    {
        public DownloadAdmx()
        {
            InitializeComponent();
        }

        private const string MicrosoftMsiDownloadLink = "https://download.microsoft.com/download/3/0/6/30680643-987a-450c-b906-a455fff4aee8/Administrative%20Templates%20(.admx)%20for%20Windows%2010%20October%202020%20Update.msi";
        private const string PolicyDefinitionsMsiSubdirectory = @"\Microsoft Group Policy\Windows 10 October 2020 Update (20H2)\PolicyDefinitions";
        private bool Downloading = false;
        public string NewPolicySourceFolder;

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    TextDestFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void DownloadAdmx_Closing(object sender, CancelEventArgs e)
        {
            if (Downloading)
                e.Cancel = true;
        }

        private void DownloadAdmx_Shown(object sender, EventArgs e)
        {
            TextDestFolder.Text = Environment.ExpandEnvironmentVariables(@"%windir%\PolicyDefinitions");
            NewPolicySourceFolder = "";
            SetIsBusy(false);
        }

        public void SetIsBusy(bool Busy)
        {
            Downloading = Busy;
            LabelProgress.Visible = Busy;
            ButtonClose.Enabled = !Busy;
            ButtonStart.Enabled = !Busy;
            TextDestFolder.Enabled = !Busy;
            ButtonBrowse.Enabled = !Busy;
            ProgressSpinner.MarqueeAnimationSpeed = Busy ? 100 : 0;
            ProgressSpinner.Style = Busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            ProgressSpinner.Value = 0;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            void setProgress(string Progress)
            {
                Invoke(new Action(() => LabelProgress.Text = Progress));
            }

            LabelProgress.Text = "";
            SetIsBusy(true);
            string destination = TextDestFolder.Text;
            bool isAdmin = false;
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                isAdmin = new System.Security.Principal.WindowsPrincipal(identity).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }

            void takeOwnership(string Folder)
            {
                var folderInfo = new DirectoryInfo(Folder);
                var dacl = FileSystemAclExtensions.GetAccessControl(folderInfo);
                var adminSid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinAdministratorsSid, null);
                dacl.SetOwner(adminSid);
                FileSystemAclExtensions.SetAccessControl(folderInfo, dacl);
                dacl = FileSystemAclExtensions.GetAccessControl(folderInfo);
                var allowRule = new System.Security.AccessControl.FileSystemAccessRule(adminSid, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
                dacl.AddAccessRule(allowRule);
                FileSystemAclExtensions.SetAccessControl(folderInfo, dacl);
            };
            void moveFilesInDir(string Source, string Dest, bool InheritAcl)
            {
                bool creatingNew = !Directory.Exists(Dest);
                Directory.CreateDirectory(Dest);
                if (isAdmin)
                {
                    if (creatingNew & InheritAcl)
                    {
                        var dirAcl = new System.Security.AccessControl.DirectorySecurity();
                        dirAcl.SetAccessRuleProtection(false, true);
                        FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(Dest), dirAcl);
                    }
                    else if (!creatingNew)
                    {
                        takeOwnership(Dest);
                    }
                }

                foreach (var file in Directory.EnumerateFiles(Source))
                {
                    string plainFilename = Path.GetFileName(file);
                    string newName = Path.Combine(Dest, plainFilename);
                    if (File.Exists(newName))
                        File.Delete(newName);
                    File.Move(file, newName);
                    if (isAdmin)
                    {
                        var fileAcl = new System.Security.AccessControl.FileSecurity();
                        fileAcl.SetAccessRuleProtection(false, true);
                        var newFileInfo = new FileInfo(newName);
                        newFileInfo.SetAccessControl(fileAcl);
                    }
                }
            };
            Task.Factory.StartNew(() =>
            {
                string failPhase = "create a scratch space";
                try
                {
                    string tempPath = Environment.ExpandEnvironmentVariables(@"%localappdata%\PolicyPlusAdmxDownload\");
                    Directory.CreateDirectory(tempPath);
                    failPhase = "download the package";
                    setProgress("Downloading MSI from Microsoft...");
                    string downloadPath = tempPath + "W10Admx.msi";
                    using (var webcli = new System.Net.WebClient())
                    {
                        webcli.DownloadFile(MicrosoftMsiDownloadLink, downloadPath);
                    }

                    failPhase = "extract the package";
                    setProgress("Unpacking MSI...");
                    string unpackPath = tempPath + "MsiUnpack";
                    var proc = Process.Start("msiexec", "/a \"" + downloadPath + "\" /quiet /qn TARGETDIR=\"" + unpackPath + "\"");
                    proc.WaitForExit();
                    if (proc.ExitCode != 0)
                        throw new Exception(); // msiexec failed
                    File.Delete(downloadPath);
                    if (Directory.Exists(destination) & isAdmin)
                    {
                        failPhase = "take control of the destination";
                        setProgress("Securing destination...");
                        Privilege.EnablePrivilege("SeTakeOwnershipPrivilege");
                        Privilege.EnablePrivilege("SeRestorePrivilege");
                        takeOwnership(destination);
                    }

                    failPhase = "move the ADMX files";
                    setProgress("Moving files to destination...");
                    string unpackedDefsPath = unpackPath + PolicyDefinitionsMsiSubdirectory;
                    string langSubfolder = System.Globalization.CultureInfo.CurrentCulture.Name;
                    moveFilesInDir(unpackedDefsPath, destination, false);
                    string sourceAdmlPath = unpackedDefsPath + @"\" + langSubfolder;
                    if (Directory.Exists(sourceAdmlPath))
                        moveFilesInDir(sourceAdmlPath, destination + @"\" + langSubfolder, true);
                    if (langSubfolder != "en-US")
                    {
                        // Also copy the English language files as a fallback
                        moveFilesInDir(unpackedDefsPath + @"\en-US", destination + @"\en-US", true);
                    }

                    failPhase = "remove temporary files";
                    setProgress("Cleaning up...");
                    Directory.Delete(tempPath, true);
                    setProgress("Done.");
                    Invoke(new Action(() =>
                    {
                        SetIsBusy(false);
                        if (Interaction.MsgBox("ADMX files downloaded successfully. Open them now?", MsgBoxStyle.YesNo | MsgBoxStyle.Question) == MsgBoxResult.Yes)
                        {
                            NewPolicySourceFolder = destination;
                        }

                        DialogResult = DialogResult.OK;
                    }));
                }
                catch (Exception)
                {
                    Invoke(new Action(() =>
                    {
                        SetIsBusy(false);
                        Interaction.MsgBox("Failed to " + failPhase + ".", MsgBoxStyle.Exclamation);
                    }));
                }
            });
        }
    }
}