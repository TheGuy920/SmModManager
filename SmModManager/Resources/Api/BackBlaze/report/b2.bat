cd .\Resources\Api\BackBlaze
b2 authorize-account %3 %4
b2 upload-file %2 .\report\%1 %1 > %5
