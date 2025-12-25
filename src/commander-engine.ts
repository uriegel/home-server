import { Request, Response } from "express"
import { readdir, stat } from "fs/promises"
import path from "path"
import { NextFunction } from "connect"
import { Dirent, statSync } from "fs"
import "functional-extensions"
import { AsyncEnumerable } from "functional-extensions"

type FileItem = {
    name: string
    isDirectory: boolean,
    size: number,
    isHidden: boolean,
    time: number
}

export async function getFiles(req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>, next: NextFunction) {
    const filePath = path.join("/", ...(req.params.splat ? req.params.splat : []))

    const items = AsyncEnumerable.from(readdir(filePath, {
        withFileTypes: true
    }))
        .mapAwait(async n => {
            const s = await stat(path.join(filePath, n.name))
            return {
                name: n.name,
                isDirectory: n.isDirectory(),
                isHidden: n.name.startsWith("."),
                size: s.size,
                time: s.mtimeMs
            } as FileItem
        })

    res.json(await items.await())
}
