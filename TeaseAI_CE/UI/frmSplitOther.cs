﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeaseAI_CE.UI
{
	public partial class frmSplitOther : Form
	{
		public frmSplitOther()
		{
			InitializeComponent();
		}

		private void frmSplitChat_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}
	}
}