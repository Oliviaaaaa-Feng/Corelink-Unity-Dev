// V1.0.0.0

const config = {
  ControlPort: 20012,
  /*            ControlIP: '127.0.0.1', */
  ControlIP: 'corelink.hpc.nyu.edu',
  autoReconnect: false,
  /*
    for service in a local network please replace the certificate with the appropriate version
  cert: '<corelink-tools-repo>/config/ca-crt.pem'
  */
}
const username = 'Testuser'
const password = 'Testpassword'
const corelink = require('corelink-client')

// Setup
const workspace = 'Holodeck'
const protocol = 'tcp'
const datatype = 'distance'

corelink.debug = true

let iter = 1
let direction = 1
function randomdata() {
  let output = iter.toString();
  iter += direction;
  if (iter >= 10 || iter <= 1) {
    direction *= -1;
  }
  console.log(output);
  return output;
}
const run = async () => {
  let sender
  if (await corelink.connect({ username, password }, config).catch((err) => { console.log(err) })) {
    sender = await corelink.createSender({
      workspace,
      protocol,
      type: datatype,
      metadata: { name: 'Random Data' },
    }).catch((err) => { console.log(err) })
  }
  setInterval(() => corelink.send(sender, Buffer.from(randomdata())), 1000)
}

run()
