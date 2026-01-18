# .NET Artisan-like command line tool
## Intro
After spending some time working with Laravel, I noticed upon returning to .NET that it's significantly faster to use CLI commands than to manually create every file.

## Purpose
This tool was developed to streamline and automate the creation of boilerplate files and standard procedures. It centralizes and applies consistent patterns to tasks that are usually handled manually during the development process.

## Commands
[1. Create a database model](#create-a-database-model)




### Create a database model
```sh
dn-cmd make:model <model_name>
```
If `<model_name>` is empty, it will prompt for a model name.

Once you confirm, it will create a model with the given name into the `Models` folder.
