import { Request, Response } from "express"
import { readdir, stat } from "fs/promises"
import path from "path"
import { NextFunction } from "connect"

export async function serveFile(directory: string, req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>, next: NextFunction) {
    const filePath = path.join(directory, ...(req.params.splat ? req.params.splat : []))
    if (await isDirectory(filePath)) {

        const items = await readdir(filePath, {
            withFileTypes: true
        })
        const [dirs, files] = items.partition(n => n.isDirectory())
        res.json({
            directories: dirs.map(n => n.name),
            files: files.map(n => n.name)
        })
    } else
        return next()
}

export async function isDirectory(path: string) {
    return (await stat(path)).isDirectory()
}