namespace Extract.Utilities.Forms
{
    partial class BetterDataGridViewRow<T>
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_dataItem != null)
                {
                    lock (_lock)
                    {
                        if (_ownDataItem)
                        {
                            System.IDisposable disposable = _dataItem as System.IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }

                        _dataItem = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
