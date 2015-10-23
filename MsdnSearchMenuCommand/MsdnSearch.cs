//------------------------------------------------------------------------------
// <copyright file="MsdnSearch.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Net;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MsdnSearchMenuCommand
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class MsdnSearch : Package
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("7001a343-151a-45ec-a578-4f1e93944e8a");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly Package package;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsdnSearch"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private MsdnSearch(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException("package");
			}

			this.package = package;

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				//Use OleMenuCommand to dynamically control visibility
				//var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
				var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
				menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
				commandService.AddCommand(menuItem);
			}
		}

		private void MenuItemOnBeforeQueryStatus(object sender, EventArgs eventArgs)
		{
			OleMenuCommand menuCommand = sender as OleMenuCommand;
			if (menuCommand == null)
				return;

			DTE dte = GetGlobalService(typeof(DTE)) as DTE;
			if (dte == null)
				return;

			//Check for selected text
			TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;
			string searchText = selection.Text;
			if (string.IsNullOrEmpty(searchText))
			{
				menuCommand.Visible = false;
				return;
			}

			menuCommand.Visible = true;
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static MsdnSearch Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private IServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			Instance = new MsdnSearch(package);
		}

		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event args.</param>
		private void MenuItemCallback(object sender, EventArgs e)
		{
			DTE dte = GetGlobalService(typeof(DTE)) as DTE;
			if (dte == null)
				return;

			TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;
			string searchText = selection.Text;

			if (string.IsNullOrEmpty(searchText))
				return;

			string url =
				"https://social.msdn.microsoft.com/Search/en-US?query=" + WebUtility.UrlEncode(searchText);

			dte.ItemOperations.Navigate(url);
		}
	}
}
