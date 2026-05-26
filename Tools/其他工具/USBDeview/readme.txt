


USBDeview v3.10
Copyright (c) 2006 - 2026 Nir Sofer
Web Site: https://www.nirsoft.net



Description
===========

USBDeview is a small utility that lists all USB devices that currently
connected to your computer, as well as all USB devices that you
previously used.
For each USB device, extended information is displayed: Device
name/description, device type, serial number (for mass storage devices),
the date/time that device was added, VendorID, ProductID, and more...
USBDeview also allows you to uninstall USB devices that you previously
used, and disconnect USB devices that are currently connected to your
computer.
You can also use USBDeview on a remote computer, as long as you login to
that computer with admin user.

License
=======

This utility is released as freeware. You are allowed to freely
distribute this utility via floppy disk, CD-ROM, Internet, or in any
other way, as long as you don't charge anything for this. If you
distribute this utility, you must include all files in the distribution
package, without any modification !



Disclaimer
==========

The software is provided "AS IS" without any warranty, either expressed
or implied, including, but not limited to, the implied warranties of
merchantability and fitness for a particular purpose. The author will not
be liable for any special, incidental, consequential or indirect damages
due to loss of data or any other reason.



System Requirement
==================


* This utility works on Windows 2000, Windows XP, Windows 2003, Windows
  Vista, Windows Server 2008, Windows 7, Windows 8, Windows 10, and
  Windows 11. Both 32-bit and 64-bit systems are supported. Windows 98/ME
  is not supported.



Known Issues
============


* In order to disable/enable USB items on x64 systems, you also must
  use the x64 version of USBDeview.
* The 'Created Date' column doesn't display correct values on Windows
  7/8/Vista/2008.
* Some USB devices with bad driver may cause USBDeview to hang. In
  order to bypass this problem, you should turn off the 'Retrieve USB
  Power/Version Information' option:
  USBDeview.exe /RetrieveUSBPower 0



Versions History
================


* Version 3.10
  o Added 'Black Background' option (Under the View menu). When it's
    turned on, the main table is displayed in black background and white
    text, instead of default system colors.
  o Added 'Save All Items' option (Shift+Ctrl+S).

* Version 3.07
  o Fixed issue: When copying data to the clipboard or exporting to
    tab-delimited file, every line contained an empty field in the end of
    the line.
  o Added 'Sort By' toolbar button.

* Version 3.06
  o Fixed a few high DPI mode issues.
  o Added 'Show Milliseconds In Time' option.

* Version 3.05
  o Added 'WCID' column, which displays the 'Compatible ID' and
    'Sub-Compatible ID' strings for WCID Devices (For example: MTP, PTP,
    WINUSB)

* Version 3.03
  o Updated USBDeview to load the usb.ids.txt file if usb.ids doesn't
    exist. (Because some Web browsers save it by default with .txt
    extension)

* Version 3.02
  o Updated to work properly in high DPI mode.
  o Added option to change the sorting column from the menu (View ->
    Sort By). Like the column header click sorting, if you click again
    the same sorting menu item, it'll switch between ascending and
    descending order. Also, if you hold down the shift key while choosing
    the sort menu item, you'll get a secondary sorting.

* Version 3.01
  o Added 'Copy Clicked Cell' option to the right-click context menu,
    which copies to the clipboard the text of cell that you right-clicked
    with the mouse.

* Version 3.00
  o Added 4 new columns: Install Time, First Install Time, Connect
    Time, Disconnect Time.
  o You can view the new fields only when you run USBDeview as
    Administrator (Elevation). You can run USBDeview as Administrator by
    pressing Ctrl+F11.
  o 'Install Time' and 'First Install Time' fields are available
    starting from Windows 7.
  o 'Connect Time' and 'Disconnect Time' fields are available only on
    Windows 8 and Windows 10.
  o These properties are stored in the Registry under
    Properties\{83da6326-97a6-4088-9453-a1923f573b29} subkey of every USB
    device, and only SYSTEM user can read them.
  o Changed the 'Created Date' column to 'Registry Time 1' and the
    'Last Plug/Unplug Date' column to 'Registry Time 2'. The change is
    made because the actual meaning of these timestamps is different from
    one system to another.

* Version 2.86
  o Added 'RegEdit Open Mode' option - 'Without Elevation' or 'With
    Elevation'.

* Version 2.85
  o Added option to create a shortcut on your desktop to
    disable,enable,disable-enable, or disconnect a USB device.
  o In order to create the shortcut, select the desired item you want
    to disable/enable/disconnect, go to File -> Create Desktop Shortcut
    and from the submenu choose the desired action for the shortcut.
  o The shortcut will work only for the USB item you selected.

* Version 2.81
  o Added 'Open Device Properties Window' option.

* Version 2.80
  o Fixed the serial number command-line options (/stop_by_serial ,
    /enable_by_serial , /disable_by_serial , /remove_by_serial) to accept
    decoded serial numbers.

* Version 2.79
  o Added 'Always On Top' option.

* Version 2.78
  o Added /cfg command-line option to start USBDeview with the
    specified config file.

* Version 2.77
  o You can now change the number of milliseconds to wait between
    disable and enable actions when using the 'Disable+Enable Selected
    Devices' option (or disable-enable command-line options).
    In order to change it - edit the config file (USBDeview.cfg) and
    change the 'DisableEnableWaitTime' value (The default is 100
    milliseconds).

* Version 2.76
  o You can now send the data to stdout by specifying empty string as
    filename, for example:
    USBDeview.exe /scomma "" | more

* Version 2.75
  o When connecting a remote computer, the computer name is now
    displayed in the window title.
  o Added 'Select All' and 'Deselect All' to the 'Column Settings'
    window.

* Version 2.74
  o Fixed bug: The /regfile , /remote and /remotefile command-line
    options stopped working properly because the USBDeview loaded the
    settings from the .cfg file...

* Version 2.73
  o USBDeview now detects the port name (COM1, COM2, COM3... ) of
    FTDI devices.

* Version 2.72
  o Added 'Friendly Name' column.

* Version 2.71
  o When running USBDeview locally, the 'Computer Name' column now
    displays the local computer name (In previous versions it was empty)

* Version 2.70
  o Added new command-line options to check whether a USB device is
    connected (USBDeview returns 0 if the device is not connected):
    /is_connected , /is_connected_by_serial , /is_connected_by_drive,
    /is_connected_by_class , /is_connected_by_pid
    For example: USBDeview.exe /is_connected
    "USB\Vid_1058&Pid_1023\8539583490834690"
  o Added new command-line options to check whether a USB device is
    disabled (USBDeview returns 0 if the device is not disabled):
    /is_disabled , /is_disabled_by_serial , /is_disabled_by_drive,
    /is_disabled_by_class , /is_disabled_by_pid
    For example: USBDeview.exe /is_disabled
    "USB\Vid_1058&Pid_1023\8539583490834690"

* Version 2.66
  o USBDeview now can start the Remote Registry service even if it's
    disabled.

* Version 2.65
  o Added option to automatically start the Remote Registry service
    on the remote machine for reading the USB devices information (In
    'Advnaced Options' window).

* Version 2.62
  o Added /RunAsAdmin command-line option. You can use it with other
    command-line options of actions that require elevation ( /remove,
    /disable , /enable , /disable_enable ), for example:
    USBDeview.exe /RunAsAdmin /disable
    "USB\Vid_1058&Pid_1023\8539583490834690"

* Version 2.61
  o Fixed bug: The 'Run As Administrator' option failed to work
    randomly on some systems.

* Version 2.60
  o Improved the detection of device connect status (Connected
    column) on Windows 10 and Windows 8.

* Version 2.55
  o Added 'Display Only Devices With Drive Letter' option.

* Version 2.52
  o USBDeview now saves the last selected font in the .cfg file.

* Version 2.51
  o Added 'Open INF File' option.

* Version 2.50
  o Added 'Driver InfSection' and 'Driver InfPath' columns.

* Version 2.46
  o Added 'Run As Administrator' option.

* Version 2.45
  o You can now choose to connect a remote computer or external
    Registry file from the 'Advanced Options' window (In previous
    versions, you could do it only from command-line)
  o Fixed to display an error message when USBDeview fails to connect
    a remote computer.

* Version 2.42
  o Added 'Capabilities' column. You can read about the meaning of
    every string inside this column in this article

* Version 2.41
  o Fixed bug: USBDeview failed to remember the last size/position of
    the main window if it was not located in the primary monitor.

* Version 2.40
  o Added 'Sort On Every Update' option.

* Version 2.37
  o Added 'Decode Serial Numbers' option. When it's turned on, a
    serial number that is encoded as hexadecimal string is decoded to
    Ascii.

* Version 2.36
  o Added secondary sorting support: You can now get a secondary
    sorting, by holding down the shift key while clicking the column
    header. Be aware that you only have to hold down the shift key when
    clicking the second/third/fourth column. To sort the first column you
    should not hold down the Shift key.

* Version 2.35
  o Added 'Reload USB Hub' option (Ctrl+R). When you select a USB hub
    item and use the 'Reload USB Hub' option, all USB devices connected
    to this hub are reconnected, including USB devices that you
    previously disconnected. Be aware that in order to use this option,
    you also have to turn on the 'Show USB Hubs' option (Ctrl+F7).

* Version 2.31
  o Added 'Copy Sorted Column Data' option, which copies to the
    clipboard the text of all selected items, but only the column that is
    currently sorted.

* Version 2.30
  o Fixed bug on Windows Vista/7/8 with UAC turned on: USBDeview
    failed to remove USB devices when running without elevation.

* Version 2.29
  o Fixed to display the correct Power value of USB 3.0 devices.

* Version 2.28
  o Fixed to display date/time values according to daylight saving
    time settings.

* Version 2.27
  o You can now use the 'Open In RegEdit' features on Windows
    Vista/7/8 (with UAC turned on) without running USBDeview.exe as
    Administrator. Elevation window will appear to confirm every action.

* Version 2.26
  o Update for Windows 7/8/Vista/2008: USBDeview now extracts the
    real device name directly from hardware when the device is connected
    and the 'Retrieve USB Power/Version Information' option is turned on.

* Version 2.25
  o Added new command-line options to disable and then enable again a
    device: /disable_enable, /disable_enable_by_serial ,
    /disable_enable_by_drive , /disable_enable_by_class ,
    /disable_enable_by_pid , /disable_enable_all
  o You can now specify the device Instance ID string in the
    following command-line options: /disable, /enable, /remove,
    /disable_enable, /stop
    For example: USBDeview.exe /disable
    "USB\Vid_1058&Pid_1023\8539583490834690"
  o You can now use the disable/enable/uninstall features on Windows
    Vista/7/8 (with UAC turned on) without running USBDeview.exe as
    Administrator. Elevation window will appear to confirm every action.

* Version 2.22
  o Fixed USBDeview to add header line to the csv/tab-delimited files
    when exporting the USB information from command-line. In order to add
    the header line, you can use the /AddExportHeaderLine command-line
    option, or simply turn on the 'Add Header Line To CSV/Tab-Delimited
    File' option from the user interface.

* Version 2.21
  o You can now use the disable/enable/remove command-line options
    according to the com number of a USB to serial device. For example:
    USBDeview.exe /disable_by_drive com62

* Version 2.20
  o Update for Windows 8: It seems that on Windows 8, it's impossible
    to detect from the Registry whether a device is connected or
    disconnected, so instead of using the Registry, a device is displayed
    as connected if it has power/version information. Be aware that if
    the 'Retrieve USB Power/Version Information' option is turned off on
    Windows 8, all devices will displayed as disconnected.

* Version 2.18
  o Fixed issue: The %drive% variable returned empty string for USB
    to serial devices.

* Version 2.17
  o Fixed bug: When plugging 2 USB devices with the same pid/vid,
    USBDeview disabled/enabled/removed the wrong device.

* Version 2.16
  o Added new variable to the execute command option: %usb_version%

* Version 2.15
  o Added 'Auto Size Columns+Headers' option, which allows you to
    automatically resize the columns according to the row values and
    column headers.
  o Fixed issue: The properties and the options windows opened in the
    wrong monitor, on multi-monitors system.

* Version 2.12
  o Fixed issue: USBDeview failed to detect some devices as
    connected, even if they were connected.

* Version 2.11
  o Fixed USBDeview to detect the version/power information of Etron
    USB 3.0 host controller, and possibly other USB host controllers.

* Version 2.10
  o Fixed the 'Last Plug/Unplug Date' value on Windows 7.
  o Fixed the 'Device Mfg' value on Windows 7.

* Version 2.06
  o Fixed bug: USBDeview froze for a few seconds if there was a
    disconnected network drive on the system.

* Version 2.05
  o Fixed the speed test error messages of USB 3.0 devices. Also, you
    can now submit the speed tests of USB 3.0 devices to
    http://usb3speed.nirsoft.net Web site

* Version 2.00
  o Changed the USB status icons so it'll be much easier to
    distinguish between them, especially on small LCD monitors.

* Version 1.97
  o Added more accelerator keys.
  o Fixed the minimum size of Advanced Options window.
  o Added a few more command-line options.

* Version 1.96
  o Fixed to USBDeview to detect USB hubs (and probably some other
    devices) as connected.

* Version 1.95
  o Added 'USB Version' column, which displays the USB version of the
    device. This column is active only when 'Retrieve USB Power/Version
    Information' option is turned on.

* Version 1.92
  o Added 'Turn Off Device On Disable/Remove' option, only for
    Windows 7/2008/Vista. As opposed to Windows XP, Windows 7/2008/Vista
    doesn't turn off the USB device when you disable or 'Safely Remove'
    the device. This new option make a small Registry change to make
    Windows 7/2008/Vista behave like Windows XP and turn off the device
    after disable or 'Safely Remove' action. For more information: USB
    Port Remains Active for Disabled or Safely Removed USB Device.
    Be aware that this change takes effect only after reboot, and
    requires full admin rights (execute USBDeview.exe with 'Run As
    Administrator')

* Version 1.91
  o For USB To Serial devices, USBDeview now displays the port name
    (Com1, Com2, Com3,...), if it's stored in the Registry. The port name
    is displayed on the 'Drive Letter' column.

* Version 1.90
  o Improved the detection of the 'Last Plug/Unplug Date' value.

* Version 1.89
  o Added 'Mark Odd/Even Rows' option, under the View menu. When it's
    turned on, the odd and even rows are displayed in different color, to
    make it easier to read a single line.

* Version 1.88
  o Added 'Mark Connected Devices' option. When it's turned on, the
    plugged USB devices are marked with green background color.

* Version 1.87
  o Added Device Instance ID column.

* Version 1.86
  o USBDeview now displays a message with instructions to run as
    administrator when you try to disable/enable/remove a device on
    Windows 7/2008/Vista with UAC turned on.

* Version 1.85
  o Added 'Change Assigned Drive Letter' option, which allows you to
    change the drive letter that is assigned to a USB device. In the next
    time that you plug the device, the new drive letter that you chose
    will be used.

* Version 1.84
  o Added new variables to the execute command option:
    %firmware_revision%, %product_name%, %vendor_name%

* Version 1.83
  o Fixed bug: When typing commands in the 'Advanced Options' with
    quotes as the first or last letter, the quotes disappeaed when saving
    to the .cfg file.

* Version 1.82
  o The properties window now contains 2 properties columns, instead
    of one very long properties column in the previous versions.

* Version 1.81
  o Added new variables to the execute command option: %driver_file%,
    %driver_version%, and %power%

* Version 1.80
  o Added 'Firmware Revision' column.

* Version 1.77
  o Added 'Display Tray Balloon When Device Is Disconnected' option.

* Version 1.76
  o Added 'Add Header Line To CSV/Tab-Delimited File' option. When
    this option is turned on, the column names are added as the first
    line when you export to csv or tab-delimited file.

* Version 1.75
  o Added 'Driver Description' and 'Driver Version' columns.
  o The status and version of usb.ids (The VendorID/ProductID file)
    is now displayed in the right side of the bottom status bar. Clicking
    this usb.ids status will open http://www.linux-usb.org/usb.ids in
    your default Web browser.

* Version 1.70
  o Added option to execute a command when you unplug a USB device
    (In 'Advanced Options' window)

* Version 1.67
  o You can now send the USB devices information to stdout by
    specifying an empty filename ("") in the command-line. (For example:
    usbdeview.exe /stext "" >> c:\temp\usb.txt)
  o Added new variables to the execute command option: %device_type%,
    %service_name%, %device_class%, and %device_mfg%

* Version 1.65
  o Added 'Power' column, which displays the power required by the
    device in mA. (Works only for local computer)

* Version 1.60
  o Added 2 new columns: 'Device Class' and 'Device Mfg'.

* Version 1.56
  o Fix bug: when saving USB devices list from command-line,
    USBDeview saved all items, without considering the last saved
    configuration.
  o Added command-line options that controls what devices to save or
    display (/DisplayDisconnected, /DisplayHubs, /DisplayNoPortSerial,
    /DisplayNoDriver).

* Version 1.55
  o Added 3 new columns of service/driver information: Service Name,
    Service Description, Driver Filename.

* Version 1.50
  o Improved drive letter detection: USBDeview now also detect the
    drive letter of hard-disk drives connected to USB. (This improvement
    only works only for local computer)

* Version 1.48
  o Fixed bug: USBDeview didn't allow you to make a speed test when
    the drive letter of the USB drive was A: or B:

* Version 1.47
  o Fixed bug: USBDeview displayed wrong drive letters when the
    serial number or ParentId Prefix contained only one character.
  o Added a separated version for x64 systems, in order to allow you
    to disable/enable items on x64 systems.

* Version 1.46
  o Added 'ParentId Prefix' column, which shows the ID string of the
    parent device.

* Version 1.45
  o Fixed bug: On Windows Vista/7, USBDeview failed to detect that
    USB devices are disabled.
  o Added support for sorting the USB devices list from command-line.

* Version 1.43
  o Fixed issue with Windows 7/Vista: The description field displayed
    unneeded prefix string like '@oem4.inf,%athr.devicedesc.5523%'.

* Version 1.42
  o Added a few reliability checks for flash drive speed tests. If
    USBDeview detect that the final speed test result might not be
    reliable, an error message will be displayed.

* Version 1.41
  o Fixed issue: When USBDeview window is hidden and there is an icon
    in the taskbar, running USBDeview again will open the existing
    instance of USBDeview, instead of creating another one.

* Version 1.40
  o Added 'Speed Test' option for testing the read/write speed of
    your USB flash drive. Also, Added an option to publish your speed
    test results in http://usbspeed.nirsoft.net.

* Version 1.36
  o Fixed bug: USBDeview didn't display some partly installed devices.

* Version 1.35
  o Added 'Vendor Name' and 'Product Name' columns. (Requires to
    download an external USB IDs file)
  o New variables in command-string: %vid_hex% and %pid_hex% (vid and
    pid values as hexadecimal number)
  o Add icons legend in the help file and in the Web page.

* Version 1.30
  o Added new option: Show Time In GMT.

* Version 1.29
  o Fixed bug: For USB flash drives with multiple drives (like
    SanDisk Cruzer With U3), USBDeview displayed only one drive letter
    randomly. Now it can display multiple drive letters.

* Version 1.28
  o Added command-line options: /stop_by_pid, /disable_by_pid,
    /enable_by_pid, /remove_by_pid

* Version 1.27
  o Added new option: Open In RegEdit.

* Version 1.26
  o The 'Advanced Options' dialog-box is now resizable.
  o Added 'USB Hub' column (yes/no).

* Version 1.25
  o Added Disable+Enable option - gives you the affect similar to
    unplugging and then plugging back the device.
  o Added 'Open Drive' option (For USB flash drives).

* Version 1.22
  o Added new command line options: /remove_all_connected,
    /remove_all_disconnected
  o Fixed bug: using /remove_all removed also the USB hubs and caused
    the USB to stop working until restarting the computer.

* Version 1.21
  o Added new option: Select Another Font.

* Version 1.20
  o Added new command-line options to disable/enable/remove USB
    devices.

* Version 1.18
  o Fixed bug: The main window lost the focus when the user switched
    to another application and then returned back to USBDeview.

* Version 1.17
  o Fixed bug in saving as comma-delimited file when description or
    name fields contained comma character.

* Version 1.16
  o Added support for saving comma-delimited (.csv) files.
  o Added new command-line option: /scomma

* Version 1.15
  o New options: Disable/enable the selected USB devices (Works only
    with admin rights)
  o New Option: Start USBDeview As Hidden (Only when 'Put Icon On
    Tray' is turned on)

* Version 1.12
  o The configuration is now saved to a file, instead of the Registry.
  o New option: Display a balloon in the tray icon when a new device
    is connected (only for Windows XP or above).

* Version 1.11
  o Fixed bug: Changing the status of 'Put Icon On Tray' also changed
    the status of 'Show Tooltips'.

* Version 1.10
  o New Option: Put Icon On Tray
  o New Option: AutoPlay on selected devices.
  o New Option: AutoPlay When Device Is Connected.
  o New Option: Execute a command when you insert a USB device. (In
    Advanced Options)
  o New Option: Disconnect All Devices.

* Version 1.06
  o Added Disconnect/Uninstall buttons to the toolbar.

* Version 1.05
  o New command-line option: /regfile
  o New option: Display USB Hubs.

* Version 1.03 - New command-line option: /stop_by_class
* Version 1.02 - New option: Display devices without drivers.
* Version 1.01 - Fixed bug: In some computers, the USB devices list was
  loaded from the wrong hardware configuration.
* Version 1.00 - First release.



Using USBDeview
===============

USBDeview doesn't require any installation process or additional DLL
files. Just copy the executable file (USBDeview.exe) to any folder you
like, and run it.
The main window of USBDeview displays all USB devices installed on your
system. You can select one or more items, and then disconnect (unplug)
them , uninstall them, or just save the information into text/xml/html
file.



USBDeview Columns Description
=============================


* Device Name: Specifies the device name. For some device, this column
  may display meaningless name, like "USB Device". If the device name is
  meaningless, try to look at the Description column.
* Device Description: The description of the device.
* Device Type: The device type, according to USB class code. For more
  information about USB classes: USB Class Codes.
* Connected: Specifies whether the device is currently connected to
  your computer. If the device is connected, you can use the 'Disconnect
  Selected Devices' option (F9) to disconnect the device.
* Safe To Unplug: Specifies whether it's safe to unplug the device from
  the USB plug without disconnecting it first. If the value of this
  column is false, and you want to unplug this device, you must first
  disconnect this device by using the 'Disconnect Selected Devices'
  option (F9) of USBDeview utility, or by using the 'Unplug or Eject
  Hardware' utility of Windows operating system.
* Drive Letter: Specifies the drive letter of the USB device. This
  column is only relevant to USB flash memory devices and to USB CD/DVD
  drives. Be aware that USBDeview cannot detect drive letters of USB
  hard-disks.
* Serial Number: Specifies the serial number of the device. This column
  is only relevant to mass storage devices (flash memory devices, CD/DVD
  drives, and USB hard-disks).
* Registry Time 1: This date/time value is taken from the timestamp of
  the device Registry key. In many cases, this date/time value represents
  the time that you first plugged the device to the USB port. However, be
  aware that in some circumstances this value may be wrong. Also, On
  Windows 7, this value is initialized with the current date/time on
  every reboot.
* Registry Time 2: This date/time value is taken from the timestamp of
  the device Registry key. On some systems this time values Specifies the
  last time that you plugged/unplugged the device. This date value is
  lost when you restart the computer.
* VendorID/ProductID: Specifies the VendorID and ProductID of the
  device. For unofficial list of VendorID/ProductID, click here.
* USB Class/Subclass/Protocol: Specifies the Class/Subclass/Protocol of
  the device according to USB specifications. For more information about
  USB classes: USB Class Codes.
* Hub/Port: Specifies the hub number and port number that the device
  was plugged into. This value is empty for mass storage devices.
* Install Time: Specifies the date/time when the USB device was
  installed. USBDeview can read this property only when you run it as
  Administrator. This property is stored under
  Properties\{83da6326-97a6-4088-9453-a1923f573b29} subkey, property
  number is 0064.
* First Install Time: Specifies when the USB device was installed in
  the first time. USBDeview can read this property only when you run it
  as Administrator. This property is stored under
  Properties\{83da6326-97a6-4088-9453-a1923f573b29} subkey, property
  number is 0065.
* Connect Time: Specifies the last time that the USB device was
  plugged. USBDeview can read this property only when you run it as
  Administrator. This property is available only on Windows 11/10/8. This
  property is stored under
  Properties\{83da6326-97a6-4088-9453-a1923f573b29} subkey, property
  number is 0066.
* Disconnect Time: Specifies the last time that the USB device was
  unplugged. This property is available only on Windows 11/10/8.
  USBDeview can read this property only when you run it as Administrator.
  This property is stored under
  Properties\{83da6326-97a6-4088-9453-a1923f573b29} subkey, property
  number is 0067.



USBDeview Icons Legend
======================




The device is not connected.


The device is connected. It's safe to physically unplug the device
without disconnecting it.


The device is connected. You must disconnect the device from USBDeview or
from Windows "Safely Remove Hardware" option before you physically unplug
it.


The device is disabled.



'Vendor Name' And 'Product Name' Columns
========================================

Starting from version 1.35, you can view the vendor name and product name
of your USB devices. In order to use this feature, you have to download
the USB ID's list of Stephen J. Gowdy (http://www.linux-usb.org/usb.ids)
and put the file in the same folder of USBDeview.exe
The name of the file must remain as 'usb.ids'.



Speed Test for USB Flash Drives
===============================

Starting from version 1.40, USBDeview allows you to test the read/write
speed of your USB flash drive. The test is made by writing a large file
(named $speed_test_nirsoft$.dat) into your USB flash drive, and then
reading it back for testing the read speed. Be aware that you need at
least 100 MB of free disk space in order to successfully make this speed
test. Also, be aware that this test is made with sequential read and
write operations. When using multiple small files, the read/write
performances are usually much lower than sequential read/write.

In order to make a speed test to your flash drive, simply select the
desired item in the main window, and then choose the 'Speed Test' option
from the File menu, or press Ctrl+T. In the speed test window, verify
that the selected drive letter is the right one, and then press the
'Start Test' button. Wait a few seconds until the write and read tests
are finished.



Publishing Your Speed Test Result
=================================

If you want to share your speed test results, so other people that want
to purchase a USB flash drive will be able to easily compare the speed of
different flash drives, you can use the 'Publish Test Result' option to
do that.

When you successfully finish a speed test, the 'Publish Test Result'
button is automatically enabled. Clicking this button opens a new window
that displays all the data that will be sent to
http://usbspeed.nirsoft.net Web site, and ask you the confirm the speed
test publishing. Be aware that only the data displayed in this window is
transmitted to http://usbspeed.nirsoft.net Web site. USBDeview doesn't
send the serial number of your USB device or any other information that
may violate your privacy.



AutoPlay
========

Starting from version 1.10, USBDeview allows you to automatically run a
file on USB flash memory devices, according to AutoRun.inf filename. You
can manually activate the AutoPlay feature by selecting the desired
device and pressing F8. Alternatively, You can check the "AutoPlay When
Device Is Connected" option, and then the AutoPlay feature will be
automatically activated when you insert a USB flash memory device.
Be aware that this feature only works if USBDeview detects the drive
letter of the device.



Execute a command when you insert a USB device
==============================================

Starting from version 1.10, USBDeview allows you to execute a command
from exe/cmd/bat file in every time that insert a USB device to your
system. In order to use this feature, go to 'Options->Advanced Options',
select the 'Execute..' check-box, and type the exe/cmd file that you want
to run. You can also use the following special variables in this command
string: %device_name%, %device_desc%, %drive%, %serial_number%, %vid%,
%pid%, %vid_hex%, %pid_hex% , %device_type%, %service_name%,
%device_class%, %device_mfg%
USBDeview will automatically replace these variables with the appropriate
data of the device that you inserted. For example, if you want to pass to
your exe file 2 parameters - the serial number of the device, and the
drive letter:
c:\temp\test.exe "%serial_number%" "%drive%"



Known Limitations/Issues
========================


* For all devices except 'Mass Storage' Devices: If you plug a device
  to one USB port, and then you plug the same device to another USB port,
  a new record of the same device will be created for each USB port that
  you plug. So if you see multiple lines for exactly the same device, it
  means that you plugged it to different USB ports. The USB ports that
  you plugged are specified in 'Hub/Port' column.
  However, for 'Mass Storage' device, only one record is created for each
  device, even if you plug it to different USB port.
* When a USB device is added/removed on your local computer, USBDeview
  utility automatically updates the list of USB devices.
  But... If you USBDeview is connected to a remote computer, the list
  won't refresh automatically when a device is added or removed. You must
  refresh it manually, by pressing F5 key.



Connecting To Remote Computer
=============================

The following command-line options allows you to connect to remote
computers. You must login to the remote computer with admin user in order
to use these options.
* /remote <\\Computer Name>
  Allows you to connect a single remote computer.
  For Example:
  USBDeview.exe /remote \\MyComp
* /remotefile <Computers List File>
  Allows you to connect multiple computers, and view all their USB
  activity in one window. The computers list file should be a simple
  Ascii text file with computer names separated by colon, semicolon,
  space, tab characters or CRLF.
  For Example:
  USBDeview.exe /remotefile "c:\temp\comp.txt"
In order to successfully get full admin access to the remote computer,
read this Blog post: How to connect a remote Windows 7/Vista/XP computer
with NirSoft utilities.



Connecting To external SYSTEM registry file
===========================================

If you have the 'SYSTEM' registry file of external operating system, you
can use the following command-line option to read the USB devices list
from it:
/regfile <SYSTEM Registry File>

For Example:
USBDeview.exe /regfile "c:\temp\regfiles\SYSTEM"
USBDeview.exe /regfile "d:\windows\system32\config\SYSTEM"

This option has some limitations:
* You cannot read a Registry file of Windows XP/2003/Vista from Windows
  2000 Machine.
* USBDeview works in read-only mode. (You cannot uninstall a device
  from external file)



Stop/Disconnect Command-Line Options
====================================


* /stop {/showmsg} {\\RemoteComputer} <Device Name>
  Stop/disconnect a USB device by specifying its name or description.
  You don't have to specify the whole name or description. You can also
  specify only a part of the name or description string, and USBDeview
  will identify it. For example, If the device name is 'Kingston
  DataTraveler 2.0', you can simply put 'kingston' or 'DataTraveler' in
  the device name parameter, and USBDeview will disconnect the right
  device. You can also specify the instance ID string of the device.
  If you want that a message/balloon will be displayed after the device
  is disconnected, add the '/showmsg' parameter. (Windows 2000 displays a
  message-box, Windows XP displays a balloon)
  If you want to disconnect a device in a remote computer, specify the
  remote computer name, with \\ prefix. You must login to the remote
  computer with admin rights in order to do that.
  Examples:
  USBDeview.exe /stop /showmsg "DVD RW"
  USBDeview.exe /stop "DataTraveler"
  USBDeview.exe /stop \\comp01 "SanDisk Cruzer"
  USBDeview.exe /stop "USB\Vid_1058&Pid_1023\8539583490834690"


* /stop_by_serial {/showmsg} {\\RemoteComputer} <Device Serial>
  Similar to /stop command, but instead of specifying a device name, you
  should specify the serial number of the device. Works only with mass
  storage devices.
  Examples:
  USBDeview.exe /stop_by_serial ea051261
  USBDeview.exe /stop_by_serial /showmsg 876534513


* /stop_by_drive {/showmsg} {\\RemoteComputer} <Drive Letter>
  Stop/disconnect a USB device by specifying its drive letter. Works only
  with USB flash drives and CD/DVD devices.
  Examples:
  USBDeview.exe /stop_by_drive g:
  USBDeview.exe /stop_by_drive /showmsg f:
  USBDeview.exe /stop_by_drive /showmsg \\comp02 t:


* /stop_by_class {/showmsg} {\\RemoteComputer} <USB Class;USB
  SubClass;USB Protocol>
  Stop/disconnect a USB device by specifying the class/subclass/protocol
  combination.
  For example, if you want to stop all 'Mass Storage' devices, you should
  specify the following class/subclass/protocol combination: 08;06;50
  Examples:
  USBDeview.exe /stop_by_class /showmsg 08;06;50
  USBDeview.exe /stop_by_class 03;01;02


* /stop_by_pid {/showmsg} {\\RemoteComputer} <VendorID;ProductID>
  Stop/disconnect a USB device by specifying the VendorID/ProductID. You
  can specify only the VendorID if you want to disconnect all devices
  with that VendorID.
  Examples:
  USBDeview.exe /stop_by_pid /showmsg 13fe;1a00
  USBDeview.exe /stop_by_pid 04f9


* /stop_all {/showmsg} {\\RemoteComputer}
  Stop/disconnect all USB devices currently connected to your computer.





Disable/Enable/Remove Command-Line Options
==========================================

Starting from version 1.20, you can also use the following commands to
disable, enable or remove USB devices from command-line:
* /disable {\\RemoteComputer} <Device Name>
* /disable_by_serial {\\RemoteComputer} <Device Serial>
* /disable_by_drive {\\RemoteComputer} <Drive Letter>
* /disable_by_class {\\RemoteComputer} <USB Class;USB SubClass;USB
  Protocol>
* /disable_by_pid {\\RemoteComputer} <VendorID;ProductID>
* /disable_all {\\RemoteComputer}
* /enable {\\RemoteComputer} <Device Name>
* /enable_by_serial {\\RemoteComputer} <Device Serial>
* /enable_by_drive {\\RemoteComputer} <Drive Letter>
* /enable_by_class {\\RemoteComputer} <USB Class;USB SubClass;USB
  Protocol>
* /enable_by_pid {\\RemoteComputer} <VendorID;ProductID>
* /enable_all {\\RemoteComputer}
* /disable_enable {\\RemoteComputer} <Device Name>
* /disable_enable_by_serial {\\RemoteComputer} <Device Serial>
* /disable_enable_by_drive {\\RemoteComputer} <Drive Letter>
* /disable_enable_by_class {\\RemoteComputer} <USB Class;USB
  SubClass;USB Protocol>
* /disable_enable_by_pid {\\RemoteComputer} <VendorID;ProductID>
* /disable_enable_all {\\RemoteComputer}
* /remove {\\RemoteComputer} <Device Name>
* /remove_by_serial {\\RemoteComputer} <Device Serial>
* /remove_by_drive {\\RemoteComputer} <Drive Letter>
* /remove_by_class {\\RemoteComputer} <USB Class;USB SubClass;USB
  Protocol>
* /remove_by_pid {\\RemoteComputer} <VendorID;ProductID>
* /remove_all {\\RemoteComputer}
* /remove_all_connected - Remove all connected USB devices.
* /remove_all_disconnected - Remove all disconnected USB devices.
Disable, enable and remove actions require elevation ('Run As
Administrator'). You can use the above command-line options with
elevation by adding /RunAsAdmin to the command, for example:
USBDeview.exe /RunAsAdmin /disable
"USB\Vid_1058&Pid_1023\8539583490834690"



Check if a device is connected/disconnected or enabled/disabled
===============================================================

Starting from version 2.70, you can check whether a device is
connected/disconnected or enabled/disabled by using the following
command-line options:
* /is_connected {\\RemoteComputer} <Device Name>
* /is_connected_by_serial {\\RemoteComputer} <Device Serial>
* /is_connected_by_drive {\\RemoteComputer} <Drive Letter>
* /is_connected_by_class {\\RemoteComputer} <USB Class;USB SubClass;USB
  Protocol>
* /is_connected_by_pid {\\RemoteComputer} <VendorID;ProductID>
* /is_disabled {\\RemoteComputer} <Device Name>
* /is_disabled_by_serial {\\RemoteComputer} <Device Serial>
* /is_disabled_by_drive {\\RemoteComputer} <Drive Letter>
* /is_disabled_by_class {\\RemoteComputer} <USB Class;USB SubClass;USB
  Protocol>
* /is_disabled_by_pid {\\RemoteComputer} <VendorID;ProductID>
When using the above commands, USBDeview returns the number of disabled
or connected devices that match the specified string.
For example, the following batch file will display 1 if the device with
serial number 7538957348957398 is connected or 0 if the device is not
connected:
USBDeview.exe /is_connected_by_serial "7538957348957398"
echo %ERRORLEVEL%




Save Command-Line Options
=========================



/stext <Filename>
Save the list of all USB devices into a regular text file.

/stab <Filename>
Save the list of all USB devices into a tab-delimited text file.

/scomma <Filename>
Save the list of all USB devices into a comma-delimited text file.

/stabular <Filename>
Save the list of all USB devices into a tabular text file.

/shtml <Filename>
Save the list of all USB devices into HTML file (Horizontal).

/sverhtml <Filename>
Save the list of all USB devices into HTML file (Vertical).

/sxml <Filename>
Save the list of all USB devices to XML file.

/sort <column>
This command-line option can be used with other save options for sorting
by the desired column. If you don't specify this option, the list is
sorted according to the last sort that you made from the user interface.
The <column> parameter can specify the column index (0 for the first
column, 1 for the second column, and so on) or the name of the column,
like "Device Name" and "Description". You can specify the '~' prefix
character (e.g: "~Description") if you want to sort in descending order.
You can put multiple /sort in the command-line if you want to sort by
multiple columns.

Examples:
USBDeview.exe /shtml "f:\temp\usb-list.html" /sort 2 /sort ~1
USBDeview.exe /shtml "f:\temp\usb-list.html" /sort "Device Type" /sort
"Device Name"

/nosort
When you specify this command-line option, the list will be saved without
any sorting.

/DisplayDisconnected <0 | 1>
Specifies whether to show disconnected devices. 0 = No, 1 = Yes.

/DisplayNoPortSerial <0 | 1>
Specifies whether to show devices without port/serial number. 0 = No, 1 =
Yes.

/DisplayNoDriver <0 | 1>
Specifies whether to show devices without driver. 0 = No, 1 = Yes.

/DisplayHubs <0 | 1>
Specifies whether to show USB hubs. 0 = No, 1 = Yes.

/RetrieveUSBPower <0 | 1>
Specifies whether to retrieve USB power/version information.

/MarkConnectedDevices <0 | 1>
Specifies whether to mark connected devices.

/TrayIcon <0 | 1>
Specifies whether to start USBDeview with a tray icon.

/AddExportHeaderLine <0 | 1>
Specifies whether to add header line when exporting the USB information
to csv/tab-delimited file.

/cfg <Filename>
Start USBDeview with the specified configuration file. For example:
USBDeview.exe /cfg "c:\config\udv.cfg"
USBDeview.exe /cfg "%AppData%\USBDeview.cfg"



Translating USBDeview to other languages
========================================

In order to translate USBDeview to other language, follow the
instructions below:
1. Run USBDeview with /savelangfile parameter:
   USBDeview.exe /savelangfile
   A file named USBDeview_lng.ini will be created in the folder of
   USBDeview utility.
2. Open the created language file in Notepad or in any other text
   editor.
3. Translate all string entries to the desired language. Optionally,
   you can also add your name and/or a link to your Web site.
   (TranslatorName and TranslatorURL values) If you add this information,
   it'll be used in the 'About' window.
4. After you finish the translation, Run USBDeview, and all translated
   strings will be loaded from the language file.
   If you want to run USBDeview without the translation, simply rename
   the language file, or move it to another folder.



Feedback
========

If you have any problem, suggestion, comment, or you found a bug in my
utility, you can send a message to nirsofer@yahoo.com
