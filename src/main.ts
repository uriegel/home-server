import express from "express"
import { Router } from "express"
import { readdir, stat } from "fs/promises"
import serveStatic from "serve-static"
import { Request } from "express"
import morgan from "morgan"
import "functional-extensions"
import path from "path"

console.log("Server fährt hoch...")

const PORT = process.env.PORT || 9865
const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
const RELATIVE_URL = process.env.RELATIVE_URL || '/media'

console.log("VIDEO_PATH", VIDEO_PATH)
console.log("MUSIC_PATH", MUSIC_PATH)

const router = Router()

const app = express()
app.use( morgan(":method :url :status :res[content-length] - :response-time ms"))
app.use(router)

const server = app.listen(PORT, () => console.log(`Listening on ${PORT}`))

process.on('SIGTERM', shutdown)
process.on('SIGINT', shutdown)

function shutdown() {
    console.log('SIGTERM signal received: closing HTTP server')
    server.close(() => console.log('Server herunter gefahren'))
}

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

router.get('/diskneeded', async (req, res) => res.sendStatus(200))
router.get('/accessdisk', async (req, res) => res.sendStatus(200))
router.use('/video', serveStatic(VIDEO_PATH))

async function isDirectory(path: string) {
    return (await stat(path)).isDirectory()
}

// router.get('/media/video/video/*', async (req, res) => {
//     const url = req.url.substr(RELATIVE_URL.length + 7)
//     const filePath = `${VIDEO_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
//     try {
//         const contentType = filePath.endsWith(".mkv") ? 'video/mkv' : 'video/mp4'
//         await serveRange(req, res, filePath, contentType)
//     }
//     catch (err: any) {
//         // Handle file not found
//         if (err !== null && err.code === 'ENOENT') {
//             res.sendStatus(404)
//         }
//     }
// })

// async function serveAudioTrack(req: express.Request, res: express.Response, track: string) {
//     await serveRange(req, res, track, "audio/mp3")
// }

// async function serveRange(req: express.Request, res: express.Response, track: string, contentType: string) {
//     const stat = await fs.stat(track)
//     const fileSize = stat.size
//     const range = req.headers.range

//     if (range) {
//         const parts = range.replace(/bytes=/, "").split("-");

//         const start = parseInt(parts[0], 10);
//         const end = parts[1] ? parseInt(parts[1], 10) : fileSize-1;
        
//         const chunksize = (end-start)+1;
//         const file = fsAll.createReadStream(track, {start, end});
//         const head = {
//             'Content-Range': `bytes ${start}-${end}/${fileSize}`,
//             'Accept-Ranges': 'bytes',
//             'Content-Length': chunksize,
//             'Content-Type': contentType,
//         }
        
//         res.writeHead(206, head);
//         file.pipe(res);
//     } else {
//         const head = {
//             'Content-Length': fileSize,
//             'Content-Type': contentType,
//         }

//         res.writeHead(200, head);
//         fsAll.createReadStream(track).pipe(res);
//     }
// }

// async function serveAlbums(res: express.Response, directory: string) {
//     const files = await fs.readdir(directory)  
//     res.send(JSON.stringify({ files }))
// }

// async function serveMusic(req: express.Request, res: express.Response) {
//     try {
//         const url = req.url.substr(RELATIVE_URL.length + 7)
//         const file = `${MUSIC_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
//         if (file.toLowerCase().endsWith(".mp3")) 
//             await serveAudioTrack(req, res, file)
//         else
//             await serveAlbums(res, file)
//     }
//     catch (err) {
//         // Handle file not found
//         if (err !== null && err.code === 'ENOENT') {
//             res.sendStatus(404);
//         }
//     }
// }

// router.route(`${RELATIVE_URL}/music`).get(serveMusic)
// router.route(`${RELATIVE_URL}/music/*`).get(serveMusic)
