import { Request, Response, Router } from "express"
import { readdir, stat } from "fs/promises"
import serveStatic from "serve-static"
import path from "path"
import { NextFunction } from "connect"

export const router = Router()

const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
const PICTURE_PATH = process.env.PICTURE_PATH || '/video'
console.log("VIDEO_PATH", VIDEO_PATH)
console.log("MUSIC_PATH", MUSIC_PATH)
console.log("Picture_PATH", PICTURE_PATH)

router.get('/video{/*splat}', (req, res, n) => serveFile(VIDEO_PATH, req, res, n))
router.use('/video', serveStatic(VIDEO_PATH))

router.get('/pics{/*splat}', (req, res, n) => serveFile(PICTURE_PATH, req, res, n))
router.use('/pics', serveStatic(PICTURE_PATH))

router.get('/music{/*splat}', (req, res, n) => serveFile(MUSIC_PATH, req, res, n))
router.use('/music', serveStatic(MUSIC_PATH))

router.get('/diskneeded', async (_, res) => res.sendStatus(200))
router.get('/accessdisk', async (_, res) => res.sendStatus(200))

async function serveFile(directory: string, req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>, next: NextFunction) {
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

async function isDirectory(path: string) {
    return (await stat(path)).isDirectory()
}
