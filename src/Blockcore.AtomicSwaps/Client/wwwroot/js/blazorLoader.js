(function () {
  let resourceIndex = 0;
  const fetchResponsePromises = [];
  const progressbar = document.getElementById("progressbar");
  const progressbarLabel = document.getElementById("progressbarLabel");
  const loadStart = new Date().getTime();

  if (!isAutostartDisabled()) {
    console.warn(
      "`blazor.webassembly.js` script tag doesn`t have the `autostart=false` attribute."
    );
    return;
  }

  Blazor.start({
    loadBootResource: function (type, filename, defaultUri, integrity) {
      if (type === "dotnetjs") {
        progressbarLabel.innerText = filename;
        return defaultUri;
      }

      const responsePromise = fetch(defaultUri, {
        cache: "no-cache",
        integrity: integrity,
      });
      fetchResponsePromises.push(responsePromise);
      responsePromise.then((response) => {
        if (!progressbar) {
          console.warn("Couldn't find the progressbar element on the page.");
          return;
        }

        if (!progressbarLabel) {
          console.warn(
            "Couldn't find the progressbarLabel element on the page."
          );
          return;
        }

        resourceIndex++;
        const totalResourceCount = fetchResponsePromises.length;
        const percentLoaded = Math.round(
          100 * (resourceIndex / totalResourceCount)
        );
        progressbar.style.width = `${percentLoaded}%`;
        progressbar.innerText = `${percentLoaded} % [${resourceIndex}/${totalResourceCount}]`;
        progressbarLabel.innerText = filename;
        if (percentLoaded >= 100) {
          var span = new Date().getTime() - loadStart;
          console.log(`All done in ${span} ms.`);
        }
      });

      return responsePromise;
    },
  });

  function isAutostartDisabled() {
    var wasmScript = document.querySelector(
      'script[src="_framework/blazor.webassembly.js"]'
    );
    if (!wasmScript) {
      return false;
    }

    var autostart = wasmScript.attributes["autostart"];
    return autostart && autostart.value === "false";
  }
})();
