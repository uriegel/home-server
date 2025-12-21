import { Request, Router } from "express"
import { readdir, stat } from "fs/promises"
import serveStatic from "serve-static"
import path from "path"

export const router = Router()

const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
console.log("VIDEO_PATH", VIDEO_PATH)
console.log("MUSIC_PATH", MUSIC_PATH)

router.get('/video{/*splat}', async (req: Request<{ splat?: string[] }>, res, next) => {
    const filePath = path.join(VIDEO_PATH, ...(req.params.splat ? req.params.splat : []))
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
})
router.use('/video', serveStatic(VIDEO_PATH))

router.get('/diskneeded', async (_, res) => res.sendStatus(200))
router.get('/accessdisk', async (_, res) => res.sendStatus(200))

async function isDirectory(path: string) {
    return (await stat(path)).isDirectory()
}
