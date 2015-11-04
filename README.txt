Sandbox for exploring examples from Machine Learning Projects for .NET Developers

notes:
* broke out solution directory into seperate standalone repository using
ref: https://help.github.com/articles/splitting-a-subfolder-out-into-a-new-repository/
* what worked for me -> swapped steps 6 and 7, i.e change the remote 'origin' URL first
then git push (master) branch to remote url ( default: 'origin' ), tried specifying 
command git push origin . but had to specify master explicitely to avoid push errors

useful resources:
* http://dungpa.github.io/fsharp-cheatsheet/
* http://fsharpforfunandprofit.com/troubleshooting-fsharp/
* http://fsharpforfunandprofit.com/posts/overview-of-types-in-fsharp/
* http://fsharpforfunandprofit.com/posts/defining-functions/
* http://fsharpforfunandprofit.com/posts/match-expression/
* http://fsharpforfunandprofit.com/posts/function-values-and-simple-values/
* http://blogs.msdn.com/b/chrsmith/archive/2008/06/14/function-composition.aspx
* Early gotcha was order of scripts in project http://stackoverflow.com/questions/1608240/how-do-i-reference-types-or-modules-defined-in-other-f-files
* Discussing FSI namespace handling - http://stackoverflow.com/questions/2354984/f-namespaces-modules-fs-and-fsx