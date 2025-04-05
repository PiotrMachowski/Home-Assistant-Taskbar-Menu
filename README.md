[![Community Forum](https://img.shields.io/badge/Community-Forum-41BDF5.svg?style=popout)](https://community.home-assistant.io/t/home-assistant-windows-app-home-assistant-taskbar-menu/207972)
![GitHub All Releases](https://img.shields.io/github/downloads/Piotrmachowski/Home-Assistant-Taskbar-Menu/total)<!-- piotrmachowski_support_badges_start -->
[![Ko-Fi][ko_fi_shield]][ko_fi]
[![buycoffee.to][buycoffee_to_shield]][buycoffee_to]
[![PayPal.Me][paypal_me_shield]][paypal_me]
[![Revolut.Me][revolut_me_shield]][revolut_me]
<!-- piotrmachowski_support_badges_end -->

# Home Assistant Taskbar Menu

This application is a simple [Home Assistant](https://www.home-assistant.io/) client for Windows.
It can control entities from [supported domains](#application), display Home Assistant web interface, create shortcuts to your favourite actions and mirror persistent notifications.

![menu_3](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_3.png)

![browser](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/browser.png)

![notification](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/notification.png)
![search](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/search.png)

## Installation

Download installer from the latest [release](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/releases/latest).

If you do not have administrator rights you can download archive file and extract it in a desired location.

## Configuration

### Connection

A configuration window will be opened at first run. Provide an URL and [token](https://www.home-assistant.io/docs/authentication/#your-account-profile), check the configuration and save it.

![auth_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/auth_1.png)

![auth_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/auth_2.png)

After successful configuration a `config_credentials.dat` file will be created and an icon in notification tray will appear.

![icon](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/icon.png)


### Application

This application can control entities from following domains:
* `automation`
* `button`
* `climate`
* `cover`
* `fan`
* `input_boolean`
* `input_button`
* `input_number`
* `input_select`
* `light`
* `lock`
* `media_player`
* `number`
* `scene`
* `script`
* `select`
* `siren`
* `switch`
* `vacuum`

By default menu contains first 100 supported entities except automations and scripts, ordered alphabetically by entity id.

![menu_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_1.png)
![menu_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_2.png)

To configure this list use option **Configure HA Taskbar Menu**.

![view_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_1.png)
![view_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_2.png)

To add entries to view use buttons from the first row:
* **Add Entity** - adds an entity to the root menu
  
  To use friendly name from Home Assistant leave **Name** empty.
  
  ![add_entry_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/add_entry_1.png)
  
* **Add Node** - adds a submenu to the root menu

  ![add_entry_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/add_entry_2.png)

* **Add Separator** - adds a separator to the root menu

To add entries to folder click on it with right mouse button and select an option.
To remove any entry from tree click on it with right mouse button and select **Delete**.

In the second row of buttons you can configure other application features:
* Switching between **light and dark modes** (change will be applied after application restart)  

  ![menu_4](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_4.png)
  ![view_3](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_3.png)

* Enabling and disabling **persistent notifications mirroring**

## Usage

* You can open built-in browser by clicking taskbar icon with left mouse button.
* You can quickly toggle an entity (if supported) by clicking it with right mouse button.
* To open search menu start typing when mouse is over an open menu.
* You can close search window by pressing [Esc].
* Configuration is stored by default in `%APPDATA%\Home Assistant Taskbar Menu\Home Assistant Taskbar Menu` directory.
* If you have installed this application to a custom location a configuration directory will match it.
* Connection configuration is stored as an encrypted text in `config_credentials.dat`. 
You can copy it between computers to use the same connection parameters.
* View configuration is stored in `config_view.dat`.
You can copy it between computers to use the same view configuration.
* To start this application with Windows add a shortcut to `StartUp` folder. You can open it by running command `shell:startup` in Run menu (shortcut: [WIN] + [R])
* To clear browser's cache remove `browserCache` folder from config directory.


## Shortcuts

You can use this application to create shortcuts to your favourite service calls (e.g. toggle light).
* Create a shortcut to `Home Assistant Taskbar Menu.exe` file
* Add the following text to *Target* section: ` call_service light.toggle {\"entity_id\": \"light.desk\"}`. **Remember to add a backslash before every double quote!**
  ![shortcut_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/shortcut_1.png)
* You can also configure a shortcut key combination using *Shortcut key* section
  ![shortcut_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/shortcut_2.png)

## Updating

To update the application to newer version you just have to use the latest installer.

### Migration from v1.0.X.X

To use configuration from older version of application copy `config.dat` and `viewConfig.dat` to config directory (default: `%APPDATA%\Home Assistant Taskbar Menu\Home Assistant Taskbar Menu`). It is created after a first start of application.



<!-- piotrmachowski_support_links_start -->

## Support

If you want to support my work with a donation you can use one of the following platforms:

<table>
  <tr>
    <th>Platform</th>
    <th>Payment methods</th>
    <th>Link</th>
    <th>Comment</th>
  </tr>
  <tr>
    <td>Ko-fi</td>
    <td>
      <li>PayPal</li>
      <li>Credit card</li>
    </td>
    <td>
      <a href='https://ko-fi.com/piotrmachowski' target='_blank'><img height='35px' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' border='0' alt='Buy Me a Coffee at ko-fi.com' />
    </td>
    <td>
      <li>No fees</li>
      <li>Single or monthly payment</li>
    </td>
  </tr>
  <tr>
    <td>buycoffee.to</td>
    <td>
      <li>BLIK</li>
      <li>Bank transfer</li>
    </td>
    <td>
      <a href="https://buycoffee.to/piotrmachowski" target="_blank"><img src="https://buycoffee.to/btn/buycoffeeto-btn-primary.svg" height="35px" alt="Postaw mi kawę na buycoffee.to"></a>
    </td>
    <td></td>
  </tr>
  <tr>
    <td>PayPal</td>
    <td>
      <li>PayPal</li>
    </td>
    <td>
      <a href="https://paypal.me/PiMachowski" target="_blank"><img src="https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg" border="0" alt="PayPal Logo" height="35px" style="height: auto !important;width: auto !important;"></a>
    </td>
    <td>
      <li>No fees</li>
    </td>
  </tr>
  <tr>
    <td>Revolut</td>
    <td>
      <li>Revolut</li>
      <li>Credit Card</li>
    </td>
    <td>
      <a href="https://revolut.me/314ma" target="_blank"><img src="https://assets.revolut.com/assets/favicons/favicon-32x32.png" height="32px" alt="Revolut"></a>
    </td>
    <td>
      <li>No fees</li>
    </td>
  </tr>
</table>

### Powered by
[![PyCharm logo.](https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg)](https://jb.gg/OpenSourceSupport)


[ko_fi_shield]: https://img.shields.io/static/v1.svg?label=%20&message=Ko-Fi&color=F16061&logo=ko-fi&logoColor=white

[ko_fi]: https://ko-fi.com/piotrmachowski

[buycoffee_to_shield]: https://shields.io/badge/buycoffee.to-white?style=flat&labelColor=white&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAABhmlDQ1BJQ0MgcHJvZmlsZQAAKJF9kT1Iw1AUhU9TpaIVh1YQcchQnayIijhKFYtgobQVWnUweemP0KQhSXFxFFwLDv4sVh1cnHV1cBUEwR8QVxcnRRcp8b6k0CLGC4/3cd49h/fuA4R6malmxzigapaRisfEbG5FDLzChxB6MIZ+iZl6Ir2QgWd93VM31V2UZ3n3/Vm9St5kgE8knmW6YRGvE09vWjrnfeIwK0kK8TnxqEEXJH7kuuzyG+eiwwLPDBuZ1BxxmFgstrHcxqxkqMRTxBFF1ShfyLqscN7irJarrHlP/sJgXltOc53WEOJYRAJJiJBRxQbKsBClXSPFRIrOYx7+QcefJJdMrg0wcsyjAhWS4wf/g9+zNQuTE25SMAZ0vtj2xzAQ2AUaNdv+PrbtxgngfwautJa/UgdmPkmvtbTIEdC3DVxctzR5D7jcAQaedMmQHMlPSygUgPcz+qYcELoFulfduTXPcfoAZGhWSzfAwSEwUqTsNY93d7XP7d+e5vx+AIahcq//o+yoAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH5wETCy4vFNqLzwAAAVpJREFUOMvd0rFLVXEYxvHPOedKJnKJhrDLuUFREULE7YDCMYj+AydpsCWiaKu29hZxiP4Al4aWwC1EdFI4Q3hqEmkIBI8ZChWXKNLLvS0/Qcza84V3enm/7/s878t/HxGkeTaIGziP+EB918nawu7Dq1d0e1+2J2bepnk2jFEUVVF+qKV51o9neBCaugfge70keoxxUbSWjrQ+4SUyzKZ5NlnDZdzGG7w4DIh+dtZEFntDA98l8S0MYwctNGrYz9WqKJePFLq80g5Sr+EHlnATp+NA+4qLaZ7FfzMrzbMBjGEdq8GrJMZnvAvFC/8wfAwjWMQ8XmMzaW9sdevNRgd3MFhvNpbaG1u/Dk2/hOc4gadVUa7Um425qii/7Z+xH9O4jwW8Cqv24Tru4hyeVEU588cfBMgpPMI9nMFe0BkFzVOYrYqycyQgQJLwTC2cDZCPeF8V5Y7jGb8BUpRicy7OU5MAAAAASUVORK5CYII=

[buycoffee_to]: https://buycoffee.to/piotrmachowski

[buy_me_a_coffee_shield]: https://img.shields.io/static/v1.svg?label=%20&message=Buy%20me%20a%20coffee&color=6f4e37&logo=buy%20me%20a%20coffee&logoColor=white

[buy_me_a_coffee]: https://www.buymeacoffee.com/PiotrMachowski

[paypal_me_shield]: https://img.shields.io/static/v1.svg?label=%20&message=PayPal.Me&logo=paypal

[paypal_me]: https://paypal.me/PiMachowski

[revolut_me_shield]: https://img.shields.io/static/v1.svg?label=%20&message=Revolut&logo=revolut

[revolut_me]: https://revolut.me/314ma
<!-- piotrmachowski_support_links_end -->