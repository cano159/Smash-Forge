﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using SALT.PARAMS;
using System.Text.RegularExpressions;

namespace VBN_Editor
{
    public partial class PARAMEditor : DockContent
    {
        public PARAMEditor(string filename)
        {
            InitializeComponent();
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            openParam(filename);
        }

        private ParamFile p;
        private DataTable tbl;
        private int[] currentEntry = new int[2];
        private bool notSaved = false;

        private void fillTable(int groupNum, int entryNum)
        {
            currentEntry[0] = groupNum;
            currentEntry[1] = entryNum;
            tbl.Clear();
            if (p.Groups.Count > groupNum)
            {
                if (!(p.Groups[groupNum] is ParamGroup))
                {
                    int i = 0;
                    foreach (ParamEntry val in p.Groups[groupNum].Values)
                    {
                        DataRow tempRow = tbl.NewRow();
                        tempRow[0] = i;
                        tempRow[1] = val.Value;
                        tbl.Rows.Add(tempRow);
                        i++;
                    }
                }
                else
                {
                    int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
                    for (int j = 0; j < entrySize; j++)
                    {
                        DataRow tempRow = tbl.NewRow();
                        tempRow[0] = j;
                        tempRow[1] = p.Groups[groupNum].Values[entrySize * entryNum + j].Value;
                        tbl.Rows.Add(tempRow);
                    }
                }
            }
        }

        private void openParam(string f)
        {
            p = new ParamFile(f);
            for(int i = 0; i < p.Groups.Count; i++)
            {
                if (p.Groups[i] is ParamGroup) {
                    TreeNode[] children = new TreeNode[((ParamGroup)p.Groups[i]).EntryCount];
                    for (int j = 0; j < ((ParamGroup)p.Groups[i]).EntryCount; j++)
                    {
                        TreeNode child = new TreeNode("Entry [" + j + "]");
                        int[] temp1 = new int[2];
                        temp1[0] = i;
                        temp1[1] = j;
                        child.Tag = temp1;
                        children[j] = child;
                    }
                    TreeNode parent = new TreeNode("Group [" + i + "]", children);
                    int[] temp = new int[2];
                    temp[0] = i;
                    temp[1] = 0;
                    parent.Tag = temp;
                    treeView1.Nodes.Add(parent);
                }
                else
                {
                    treeView1.Nodes.Add("Group [" + i + "]");
                }
            }
            fillTable(0,0);
        }

        private void select(object sender, TreeViewEventArgs e)
        {
            fillTable(((int[])e.Node.Tag)[0], ((int[])e.Node.Tag)[1]);
        }

        private void edit(object sender, DataGridViewCellEventArgs e)
        {
            IParamCollection i = p.Groups[currentEntry[0]];
            if (i is ParamGroup)
            {
                ParamGroup p = (ParamGroup)i;
                p.Values[p.EntrySize * currentEntry[1] + e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
            }
            else
            {
                i.Values[e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
            }
            if (!notSaved)
            {
                Text += "*";
                notSaved = true;
            }
        }

        private void save()
        {
            p.Export(p.Filepath);
            notSaved = false;
            if (Text.EndsWith("*"))
                Text = Text.Substring(0, Text.Length - 1);
        }

        public void saveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Paramter Files (.bin)|*.bin|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filename = sfd.FileName;
                    p.Export(filename);
                    notSaved = false;
                    if (Text.EndsWith("*"))
                        Text = Text.Substring(0, Text.Length - 1);
                }
            }

        }
    }
}
