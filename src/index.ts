
async function serveAudioTrack(req: express.Request, res: express.Response, track: string) {
    await serveRange(req, res, track, "audio/mp3")
}

async function serveAlbums(res: express.Response, directory: string) {
    const files = await fs.readdir(directory)  
    res.send(JSON.stringify({ files }))
}

async function serveMusic(req: express.Request, res: express.Response) {
    try {
        const url = req.url.substr(RELATIVE_URL.length + 7)
        const file = `${MUSIC_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
        if (file.toLowerCase().endsWith(".mp3")) 
            await serveAudioTrack(req, res, file)
        else
            await serveAlbums(res, file)
    }
    catch (err) {
        // Handle file not found
        if (err !== null && err.code === 'ENOENT') {
            res.sendStatus(404);
        }
    }
}

router.route(`${RELATIVE_URL}/music`).get(serveMusic)
router.route(`${RELATIVE_URL}/music/*`).get(serveMusic)

