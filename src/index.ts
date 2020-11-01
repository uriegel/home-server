import * as express from 'express'
import * as fsAll from "fs"
import * as fs from "fs/promises"

const router = express.Router()
const path = '/media/Speicher/video'

router.get('/videos', async (req, res) => {
    const files = await fs.readdir(path)  
    res.send(JSON.stringify({ files }))
})

router.get('/video/*', async (req, res) => {
    const url = req.url.substr(7)
    const filePath = `${path}/${decodeURI(url)}`

    try {
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
                'Content-Type': 'video/mp4',
            }
            
            res.writeHead(206, head);
            file.pipe(res);
        } else {
            const head = {
                'Content-Length': fileSize,
                'Content-Type': 'video/mp4',
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

const app = express()
app.use(router)

const PORT = process.env.PORT || 9865

app.listen(PORT, () => console.log(`Listening on ${PORT}`))