using PakBenchmark;

string corpus = "../pak-corpus";
string outDir = "out";
for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--corpus") corpus = args[i + 1];
    else if (args[i] == "--out") outDir = args[i + 1];
}

return Runner.Run(corpus, outDir);
