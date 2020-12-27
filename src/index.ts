import * as express from 'express'
import * as fsAll from "fs"
import * as fs from "fs/promises"

const PORT = process.env.PORT || 9865
const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
const RELATIVE_URL = process.env.RELATIVE_URL || '/media'

const router = express.Router()

router.get(`${RELATIVE_URL}/video/list`, async (req, res) => {
    const files = await fs.readdir(VIDEO_PATH)  
    res.send(JSON.stringify({ files }))
})

router.get(`${RELATIVE_URL}/video/*`, async (req, res) => {
    const url = req.url.substr(RELATIVE_URL.length + 7)
    const file = `${VIDEO_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
    try {
        const filePath = fsAll.existsSync(file + ".mp4") ? file + ".mp4" : file + ".mkv"
        const contentType = filePath.endsWith(".mkv") ? 'video/mkv' : 'video/mp4'
        const stat = await fs.stat(filePath)
        
        const fileSize = stat.size
        const range = req.headers.range
    
        if (range) {
    
            const parts = range.replace(/bytes=/, "").split("-");
    
            const start = parseInt(parts[0], 10);
            const end = parts[1] ? parseInt(parts[1], 10) : fileSize-1;
            
            const chunksize = (end-start)+1;
            const file = fsAll.createReadStream(filePath, {start, end});
            const head = {
                'Content-Range': `bytes ${start}-${end}/${fileSize}`,
                'Accept-Ranges': 'bytes',
                'Content-Length': chunksize,
                'Content-Type': contentType,
            }
            
            res.writeHead(206, head);
            file.pipe(res);
        } else {
            const head = {
                'Content-Length': fileSize,
                'Content-Type': contentType,
            }
    
            res.writeHead(200, head);
            fsAll.createReadStream(filePath).pipe(res);
        }
    }
    catch (err) {
        // Handle file not found
        if (err !== null && err.code === 'ENOENT') {
            res.sendStatus(404);
        }
    }
})

async function serveMusic(req: express.Request, res: express.Response) {
    const url = req.url.substr(RELATIVE_URL.length + 7)
    const directory = `${MUSIC_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
    const files = await fs.readdir(directory)  
    res.send(JSON.stringify({ files }))
}

router.route(`${RELATIVE_URL}/music`).get(serveMusic)
router.route(`${RELATIVE_URL}/music/*`).get(serveMusic)

const app = express()
app.use(router)

app.listen(PORT, () => console.log(`Listening on ${PORT}`))