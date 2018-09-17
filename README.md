# Renamer
Small utility to rename directories, files and the content into the files to match a certain string. It detects binary files and skips them, making changes only in those text files whose name or content match the specified string.

## Usage
Let's suppose we have created an Angular project, a kind of template that we want to reuse to start other projects. First, we copy that template's directory, let's suppose its name is `starttemplate` and our new project will be `newproject`. So:

```
xcopy starttemplate newproject /s /e
```
After that, we enter inside `newproject` directory and type:

```
renamer --change starttemplate // newproject --exclude "node_modules"
```

From that, `renamer` will list all the files to be parsed and ask if you want to continue. Say `Y` and thats all.

There's another parameter `emitbom` to include the BOM mark at the beginning of every UTF8 file parsed, but normally it isn't needed.

That's all, I hope it helps.