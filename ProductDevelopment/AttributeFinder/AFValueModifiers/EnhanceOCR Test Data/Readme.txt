The stats in TestStats show the results of running the files in the "TestRegions" through the Enhance OCR value modifier with a variety of different filters and filter combinations.

The filter processes are listed as specified in the EnhanceOCR spec except that this test set includes cases where filters have been applied one after the other on top of each other rather than running them both separately and combining the two (as the Enhance OCR rule object does). These tests are indicated by using "->" in place of "+" between the two filters. The current Enhance OCR rule object is not capable of using these.

In included alonside the filter test results are the stats for the original text as well as the Enhance OCR rule object on various settings.

In the stats queries:
Accuracy = ([Total expected characters] - [Levenshtein distance of result from expected]) / [Total expected characters]
Confidence = The average confidence of the resulting OCR.
MaxImprovement = The largest gain in accuracy on any single test.
CountImproved = The number of samples whose accuracy improved over the original