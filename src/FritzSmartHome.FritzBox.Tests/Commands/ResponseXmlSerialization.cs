using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FluentAssertions;
using FritzSmartHome.Domain;
using Xunit;

namespace FritzSmartHome.FritzBox.Tests.Commands
{
	public class ResponseXmlSerialization
	{
		public class Deserialization_with_only_one_device
		{
            private readonly string _xml = @"<devicelist version=""1"" fwversion=""7.2"">
    <device identifier = ""09995 0689197"" id = ""16"" functionbitmask = ""320"" fwversion = ""04.94"" manufacturer = ""AVM"" productname = ""FRITZ!DECT 301"">           
                   <present>1</present>           
                   <txbusy>0</txbusy>           
                   <name>FRITZ!DECT 301 Wohnzimmer</name>              
                      <battery>90</battery>              
                      <batterylow>0</batterylow>              
                      <temperature>
                        <celsius>215</celsius>
                        <offset>0</offset>
                      </temperature>
                      <hkr>
                        <tist>43</tist>
                        <tsoll>44</tsoll>
                        <absenk>32</absenk>
                        <komfort>44</komfort>
                        <lock>0</lock>
                        <devicelock>0</devicelock>
                        <errorcode>0</errorcode>
                        <windowopenactiv>0</windowopenactiv>
                        <windowopenactiveendtime>0</windowopenactiveendtime>
                        <boostactive>0</boostactive>
                        <boostactiveendtime>0</boostactiveendtime>
                        <batterylow>0</batterylow>
                        <battery>90</battery>
                        <nextchange>
                        <endperiod>1608591600</endperiod>
                        <tchange>32</tchange>
                        </nextchange>
                        <summeractive>0</summeractive>
                        <holidayactive>0</holidayactive>
                        </hkr>                
                    </device>
    </devicelist>";

            private readonly DeviceList _deviceList;
            private readonly Device _device;

			public Deserialization_with_only_one_device()
			{
                var x = new XmlSerializer(typeof(DeviceList));
                using var reader = new StringReader(_xml);

                _deviceList = (DeviceList)x.Deserialize(reader);
                _device = _deviceList.SingleOrDefault();
            }

			[Fact]
			public void Count_of_devices_should_be_one()
			{
                _deviceList.Count.Should().Be(1);
			}

			[Fact]
			public void Should_deserialize_the_device()
			{
                _device.Should().NotBeNull();
			}

			[Fact]
			public void Device_Should_have_Identifier()
			{
                _device.Identifier.Should().Be("09995 0689197");
			}


			[Fact]
			public void Device_should_have_Id()
			{
                _device.Id.Should().Be("16");
			}

			[Fact]
			public void Device_should_have_FWVersion()
			{
                _device.FWVersion.Should().Be("04.94");
			}

            [Fact]
            public void Device_should_have_Manufacturer()
            {
                _device.Manufacturer.Should().Be("AVM");
            }

			[Fact]
			public void Device_should_have_ProductName()
			{
                _device.ProductName.Should().Be("FRITZ!DECT 301");
			}

			[Fact]
			public void Device_should_have_FunctionBitMask()
			{
                _device.FunctionBitMask.Should().Be(320);
			}

			[Fact]
			public void Device_should_be_present()
			{
                _device.Present.Should().Be(1);
                _device.IsPresent.Should().BeTrue();
			}
		}
    }
}
