# Generated by Django 3.2.19 on 2024-12-15 17:05

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('devices', '0002_auto_20241210_2337'),
    ]

    operations = [
        migrations.AddField(
            model_name='device',
            name='numeric_ip_famo',
            field=models.BigIntegerField(blank=True, editable=False, null=True),
        ),
    ]
