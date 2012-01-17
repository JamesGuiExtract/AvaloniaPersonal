To test run Test.fps. This FPS file uses the local database "Test" and the action "WYSIWYG".

This test will present a series of documents, both .tif and .pdf, for verification. Follow the on-document instructions to manually create and verify the documents. In cases where redactions are to be manually adjusted, attempt to make them just large enough to cover the sensitive pixels (but no larger).

After each is verified it will be displayed in redacted form in a separate image viewer with black outlines added around where the redactions should be. Verify that there are no sensitive pixels leaking from beneath the redaction and that the black outline is distinguishable from the redaction (i.e., there is some white between the redaction and the border on all sides.)

Notes:
* It turned out to not really be feasible to "manually rotate using the top/bottom grips". I actually accidentally added this test twice on all pages. IMO, you can try one of these two tests, but if you can't get it right in a short amount of time, go ahead and use another method of adjusting the redaction.
* On images with a significant amount of skew and long items to redact, its best to use the pixel-fitting mode of the word redaction tool (shift).
* Known issue: [FlexIDSCore:4214] On images with a large amout of skew, the automatically found redactions + redactions created with the word hightlight tool will not be lined up correctly.
* Known issue: [FlexIDSCore:4991] Redactions automatically sized via word redaction tool, shrink-to-fit, block-fit and line-fit modes will frequently leave too much padding on the top an bottom such that the outlines are not distinguishable from the redactions themselves.