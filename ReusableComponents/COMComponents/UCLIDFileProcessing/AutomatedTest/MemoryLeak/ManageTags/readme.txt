- Clear the MEMORY_LEAK database, add action "Test", add tags "a", "b", and "c".

- The first memory leak test will queue lots of files and apply tags a and b.

- Do not clear the database after exercising the first test.

- Reset file action status for Test to Pending.

- The second memory leak test will remove tag b.

- Do not clear the database after exercising the third test.

- Reset file action status for Test to Pending.

- The third memory leak test will toggle tag c.
