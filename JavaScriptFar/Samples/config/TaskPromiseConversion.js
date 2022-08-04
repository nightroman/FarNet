// Without EnableTaskPromiseConversion this script works incorrectly:
// instead of sleeping for 5 seconds it shows the result immediately.

function sleep(milliseconds) {
    return clr.System.Threading.Tasks.Task.Delay(milliseconds)
}

function job(proc) {
    return clr.FarNet.Tasks.Job(host.proc(0, proc))
}

async function main(seconds) {
    await sleep(seconds * 1000)
    await job(() => far.Message(`done in ${seconds} seconds`))
}

main(5)
