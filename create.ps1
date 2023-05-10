dotnet new sln
for ($i = 1; $i -le 10; $i++)
{
    dotnet new wpf -o "lab$i"
	dotnet sln add "./lab$i/lab$i.csproj"
}

dotnet new classlib -o "CommonMethods"
dotnet sln add "CommonMethods"

dotnet new gitignore
git init
git add .gitignore
git commit -m "init"