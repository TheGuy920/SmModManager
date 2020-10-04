cd .\Resources\Api\BackBlaze
b2 authorize-account %3 %4
b2 upload-file %2 .\%1 %1 > FileDetails.json
b2 delete-file-version %5