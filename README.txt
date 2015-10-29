Sandbox for exploring examples from Machine Learning Projects for .NET Developers

notes:
* broke out solution directory into seperate standalone repository using
ref: https://help.github.com/articles/splitting-a-subfolder-out-into-a-new-repository/
* what worked for me -> swapped steps 6 and 7, i.e change the remote 'origin' URL first
then git push (master) branch to remote url ( default: 'origin' ), tried specifying 
command git push origin . but had to specify master explicitely to avoid push errors