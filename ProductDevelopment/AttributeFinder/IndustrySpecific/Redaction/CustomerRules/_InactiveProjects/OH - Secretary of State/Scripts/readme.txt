*** Instructions ***

- Install and license IDShield 5.0

- Extract contents of 2007-05-16_Scripts.zip to 
  C:\Program Files\Extract Systems\Scripts folder

- Confirm that Master.rsd.etf and other rules files have been extracted to 
  C:\Program Files\Extract Systems\Rules folder
  * This location is referenced by the Auto-Redaction task in Process_Pages.fps

- Run the Master.fps file
  * Calls Process_Pages.fps to Auto-Redact pages and 
    create CSV files for OCR'd TIF's in a specific folder
  * Creates Process_done.txt file in the Source folder after 
    the sequence of tasks is finished
