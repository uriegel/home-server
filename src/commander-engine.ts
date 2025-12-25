import { Request, Response } from "express"
import { createWriteStream } from "fs"
import { readdir, stat, utimes } from "fs/promises"
import path from "path"
import "functional-extensions"
import { AsyncEnumerable } from "functional-extensions"

type FileItem = {
    name: string
    isDirectory: boolean,
    size: number,
    isHidden: boolean,
    time: number
}

export async function getFiles(req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>) {
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

export function putFile(req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>) {
    const filePath = path.join("/", ...(req.params.splat ? req.params.splat : []))
    const writeStream = createWriteStream(filePath)
    req.pipe(writeStream)
    writeStream.on('finish', async () => {
        const date = req.header("x-file-date") 
        if (date) {
            const mtimeMs = Number(date)
            if (!Number.isNaN(mtimeMs)) {
                const mtime = new Date(mtimeMs)
                // keep atime unchanged
                await utimes(filePath, mtime, mtime)
            }            
        }
        res.sendStatus(204)
    })
    writeStream.on('error', () => res.sendStatus(404))
}
